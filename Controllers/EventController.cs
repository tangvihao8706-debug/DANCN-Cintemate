using Microsoft.AspNetCore.Mvc;
using EventManager.Models;
using Microsoft.AspNetCore.Authorization;
using EventManager.Data;
using System;
using System.Linq;
using QRCoder;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public EventController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // 📋 Hiển thị danh sách phim / suất chiếu có lọc
        public IActionResult Index(string location, DateTime? date, int? genreId)
        {
            var events = _context.Events
                .Include(e => e.Genre) // ❗ Bắt buộc phải include
                .AsQueryable();

            if (!string.IsNullOrEmpty(location))
                events = events.Where(e => e.Location.Contains(location));

            if (date.HasValue)
                events = events.Where(e => e.Date.Date == date.Value.Date);

            if (genreId.HasValue && genreId.Value > 0)
                events = events.Where(e => e.GenreId == genreId);

            ViewBag.Genres = _context.Genres.ToList(); // 🔄 Cho dropdown

            var model = events.OrderByDescending(e => e.Date).ToList();
            return View(model);
        }

        // 🛠️ Trang tạo phim / suất chiếu (Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // 💾 Xử lý POST tạo phim / suất chiếu
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateEvent(EventModel model)
        {
            if (ModelState.IsValid)
            {
                _context.Events.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // 📝 Đặt vé xem phim
        [Authorize]
        public IActionResult Register(int id)
        {
            var ev = _context.Events.Find(id);
            if (ev == null) return NotFound();

            var takenSeats = _context.Registrations
                .Where(r => r.EventId == id && r.SeatNumber != null)
                .Select(r => r.SeatNumber!)
                .ToList();

            ViewBag.TakenSeats = takenSeats;

            return View(ev);
        }

        // ✅ Xác nhận đặt vé (gửi QR)
        [HttpPost]
        [Authorize]
        public IActionResult RegisterConfirmed(int id, string seatNumber)
        {
            var ev = _context.Events.FirstOrDefault(e => e.Id == id);
            if (ev == null)
                return NotFound();

            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return BadRequest("Không tìm thấy email người dùng");

            // 🔍 KIỂM TRA NGƯỜI DÙNG ĐÃ ĐẶT VÉ CHƯA
            bool alreadyRegistered = _context.Registrations
                .Any(r => r.EventId == id && r.UserId == userEmail);
            if (alreadyRegistered)
            {
                TempData["Error"] = "Bạn đã đặt vé cho suất chiếu này rồi!";
                return RedirectToAction("Details", new { id });
            }

            // 📦 KIỂM TRA SỐ LƯỢNG GHẾ ĐÃ ĐẶT
            int current = _context.Registrations.Count(r => r.EventId == id);
            if (current >= ev.MaxParticipants)
            {
                TempData["Error"] = "Suất chiếu này đã đủ chỗ.";
                return RedirectToAction("Details", new { id });
            }

            // ❗ KIỂM TRA GHẾ ĐÃ CÓ NGƯỜI CHỌN
            bool seatTaken = _context.Registrations
                .Any(r => r.EventId == id && r.SeatNumber == seatNumber);
            if (seatTaken)
            {
                TempData["Error"] = " Ghế này đã có người chọn. Vui lòng chọn ghế khác.";
                return RedirectToAction("Register", new { id });
            }

            // ✅ TẠO MÃ QR (Gồm cả ghế để check-in rõ ràng hơn)
            string qrContent = $"{ev.Id}|{userEmail}|{seatNumber}|{Guid.NewGuid()}";
            string base64 = GenerateQrCode(qrContent);
            byte[] qrBytes = Convert.FromBase64String(base64);

            // ✅ TẠO REGISTRATION
            var registration = new Registration
            {
                EventId = ev.Id,
                UserId = userEmail,
                RegisteredAt = DateTime.Now,
                SeatNumber = seatNumber,
                QrCodeBase64 = base64
            };

            _context.Registrations.Add(registration);
            _context.SaveChanges();

            // ✉️ GỬI EMAIL VÉ XEM PHIM
            try
            {
                string subject = $"[Vé xem phim] {ev.Title}";
                string body = $@"
        Chào bạn,<br><br>
        Bạn đã đặt vé thành công cho phim <strong>{ev.Title}</strong>, 
        chiếu ngày <strong>{ev.Date:dd/MM/yyyy}</strong> tại <strong>{ev.Location}</strong>.<br><br>
        <strong>🎟 Ghế của bạn:</strong> 
        <span style='color:#ff4f93;font-size:18px'><strong>{seatNumber}</strong></span><br><br>
        Vui lòng mang mã QR dưới đây đến rạp để check-in.<br><br>
        <img src='data:image/png;base64,{base64}' alt='QR Code' />";

                _emailService.SendEmailWithQr(userEmail, subject, body, qrBytes);
                Console.WriteLine("📨 Email vé xem phim kèm QR & ghế đã được gửi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(" Gửi email lỗi: " + ex.Message);
            }

            // ✅ GỬI DỮ LIỆU QUA VIEW "Cảm ơn"
            TempData["QrBase64"] = base64;
            TempData["EventName"] = ev.Title;
            TempData["Seat"] = seatNumber;

            return RedirectToAction("CamOn");
        }

        // 🙏 Trang cảm ơn
        public IActionResult CamOn()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Details(int id, int? starFilter)
        {
            var eventModel = _context.Events.FirstOrDefault(e => e.Id == id);
            if (eventModel == null) return NotFound();

            // 🔗 Lấy link Google Calendar (nhắc xem phim)
            var gcalUrl = GenerateGoogleCalendarUrl(eventModel);
            ViewData["gcalUrl"] = gcalUrl;

            // 💬 Lấy đánh giá và lọc theo số sao nếu có
            var ratingsQuery = _context.EventRatings
                .Where(r => r.EventId == id);

            if (starFilter.HasValue && starFilter.Value >= 1 && starFilter.Value <= 5)
            {
                ratingsQuery = ratingsQuery.Where(r => r.Rating == starFilter.Value);
                ViewBag.StarFilter = starFilter.Value;
            }

            ViewBag.Ratings = ratingsQuery
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            // ✅ Lấy email nếu người dùng đã đăng nhập
            string? email = User.Identity.IsAuthenticated
                ? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                : null;

            ViewBag.AlreadyRegistered = !string.IsNullOrEmpty(email) &&
                _context.Registrations.Any(r => r.EventId == id && r.UserId == email);

            // ✅ Suất chiếu đã đủ ghế chưa?
            int registeredCount = _context.Registrations.Count(r => r.EventId == id);
            ViewBag.EventFull = registeredCount >= eventModel.MaxParticipants;

            return View(eventModel);
        }

        // ⭐ Người dùng đánh giá phim
        [HttpPost]
        public IActionResult SubmitRating(int eventId, int rating, string comment)
        {
            if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(comment))
            {
                ModelState.AddModelError("", "Thông tin đánh giá không hợp lệ.");
                return RedirectToAction("Details", new { id = eventId });
            }

            var ratingModel = new EventRating
            {
                EventId = eventId,
                Rating = rating,
                Comment = comment,
                UserName = User.Identity.Name ?? "Khách",
                CreatedAt = DateTime.Now
            };

            _context.EventRatings.Add(ratingModel);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = eventId });
        }

        // 🔧 Tạo link Google Calendar (nhắc lịch xem phim)
        private string GenerateGoogleCalendarUrl(EventModel ev)
        {
            string formatDate = "yyyyMMddTHHmmssZ";
            var startUtc = ev.Date.ToUniversalTime().ToString(formatDate);
            var endUtc = ev.Date.AddHours(2).ToUniversalTime().ToString(formatDate);

            var title = Uri.EscapeDataString(ev.Title);
            var location = Uri.EscapeDataString(ev.Location);
            var details = Uri.EscapeDataString(ev.Description);

            return $"https://calendar.google.com/calendar/render?action=TEMPLATE&text={title}&dates={startUtc}/{endUtc}&details={details}&location={location}&sf=true&output=xml";
        }

        // 🔧 Sinh mã QR
        private string GenerateQrCode(string content)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(data);
            var byteArr = qrCode.GetGraphic(20);
            return Convert.ToBase64String(byteArr);
        }

        // ❎ Huỷ vé xem phim
        [HttpPost]
        [Authorize]
        public IActionResult CancelRegistration(int eventId)
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (userEmail == null) return BadRequest();

            var reg = _context.Registrations
                .FirstOrDefault(r => r.EventId == eventId && r.UserId == userEmail);

            if (reg != null)
            {
                var ev = _context.Events.FirstOrDefault(e => e.Id == eventId); // Lấy thông tin phim / suất chiếu

                _context.Registrations.Remove(reg);
                _context.SaveChanges();

                // ✉️ GỬI EMAIL HUỶ VÉ
                try
                {
                    if (ev != null)
                    {
                        string subject = "[Huỷ vé xem phim] {ev.Title}";
                        string body = $@"
                Chào bạn,<br><br>
                Bạn đã huỷ vé xem phim <strong>{ev.Title}</strong> 
                (suất chiếu ngày <strong>{ev.Date:dd/MM/yyyy}</strong> tại <strong>{ev.Location}</strong>).<br><br>
                Rất mong sẽ được phục vụ bạn ở những bộ phim tiếp theo!<br><br>
                Trân trọng,<br>CineMate.";

                        _emailService.SendEmail(userEmail, subject, body);
                        Console.WriteLine("Đã gửi email huỷ vé xem phim.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Gửi email lỗi: " + ex.Message);
                }

                TempData["Success"] = " Bạn đã huỷ vé xem phim này.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy vé để huỷ.";
            }

            return RedirectToAction("Details", new { id = eventId });
        }
    }
}
