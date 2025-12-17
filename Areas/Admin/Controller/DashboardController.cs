using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManager.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===== DASHBOARD (Admin Only) =====
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            ViewBag.EventCount = _context.Events.Count(); // tổng số phim / suất chiếu
            ViewBag.UserCount = _context.Users.Count();   // tổng số người dùng
            ViewBag.RegistrationCount = _context.Registrations.Count(); // tổng số lượt đặt vé
            return View();
        }

        // ===== QUẢN LÝ PHIM / SUẤT CHIẾU (Admin + Staff) =====
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult ManageEvents()
        {
            var events = _context.Events
                .OrderByDescending(e => e.Date)
                .ToList();

            return View(events);
        }

        [Authorize(Roles = "Admin,Staff")]
        public IActionResult CreateEvent()
        {
            ViewBag.Genres = new SelectList(_context.Genres.ToList(), "Id", "Name");
            return View();
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateEvent(EventModel model, IFormFile? mediaFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Upload poster phim hoặc trailer
                    if (mediaFile != null && mediaFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(mediaFile.FileName);
                        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

                        using (var stream = new FileStream(savePath, FileMode.Create))
                        {
                            mediaFile.CopyTo(stream);
                        }

                        model.MediaPath = "/uploads/" + fileName;
                    }

                    _context.Events.Add(model);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = " Phim / Suất chiếu đã được tạo thành công!";
                    return RedirectToAction(nameof(ManageEvents));
                }
                catch (Exception ex)
                {
                    ViewBag.Genres = new SelectList(_context.Genres.ToList(), "Id", "Name", model.GenreId);
                    TempData["ErrorMessage"] = "Lỗi khi lưu phim: " + ex.Message;
                    return View(model);
                }
            }

            // Log lỗi ModelState
            foreach (var entry in ModelState)
                foreach (var err in entry.Value.Errors)
                    Console.WriteLine($"ModelState Error - Field: {entry.Key}, Error: {err.ErrorMessage}");

            ViewBag.Genres = new SelectList(_context.Genres.ToList(), "Id", "Name", model.GenreId);
            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ!";
            return View(model);
        }

        [Authorize(Roles = "Admin,Staff")]
        public IActionResult EditEvent(int id)
        {
            var ev = _context.Events.Find(id);
            if (ev == null) return NotFound();

            ViewBag.Genres = new SelectList(_context.Genres.ToList(), "Id", "Name", ev.GenreId);
            return View(ev);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditEvent(EventModel model, IFormFile? mediaFile)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.Events.Find(model.Id);
                if (existing == null) return NotFound();

                existing.Title = model.Title;
                existing.Description = model.Description;
                existing.Date = model.Date; // ngày chiếu
                existing.Location = model.Location; // tên rạp
                existing.TicketPrice = model.TicketPrice;
                existing.MaxParticipants = model.MaxParticipants;
                existing.GenreId = model.GenreId;

                if (mediaFile != null && mediaFile.Length > 0)
                {
                    var fileName = Path.GetFileName(mediaFile.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                    using var stream = new FileStream(path, FileMode.Create);
                    mediaFile.CopyTo(stream);

                    existing.MediaPath = "/uploads/" + fileName;
                }

                _context.Events.Update(existing);
                _context.SaveChanges();

                TempData["SuccessMessage"] = " Cập nhật phim / suất chiếu thành công!";
                return RedirectToAction(nameof(ManageEvents));
            }

            ViewBag.Genres = new SelectList(_context.Genres.ToList(), "Id", "Name", model.GenreId);
            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ!";
            return View(model);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteEvent(int id)
        {
            var ev = _context.Events.Find(id);
            if (ev != null)
            {
                _context.Events.Remove(ev);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(ManageEvents));
        }

        // ========== QUẢN LÝ ĐẶT VÉ ==========
        [Authorize(Roles = "Admin")]
        public IActionResult ManageRegistrations()
        {
            var list = _context.Registrations
                .Include(r => r.Event)
                .OrderByDescending(r => r.RegisteredAt)
                .ToList();

            return View(list);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteRegistration(int id)
        {
            var reg = _context.Registrations.Find(id);
            if (reg != null)
            {
                _context.Registrations.Remove(reg);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(ManageRegistrations));
        }

        // ========== QUẢN LÝ NGƯỜI DÙNG ==========
        [Authorize(Roles = "Admin")]
        public IActionResult ManageUsers()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
                await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(ManageUsers));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (new[] { "User", "Staff", "Admin" }.Contains(role))
            {
                await _userManager.AddToRoleAsync(user, role);
                TempData["SuccessMessage"] = $" Gán quyền {role} cho {user.Email} thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Quyền không hợp lệ!";
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        // ========== QUẢN LÝ ĐÁNH GIÁ PHIM ==========
        [Authorize(Roles = "Admin")]
        public IActionResult ManageRatings()
        {
            var ratings = _context.EventRatings
                .Include(r => r.Event)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View(ratings);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteRating(int id)
        {
            var rating = _context.EventRatings.Find(id);
            if (rating != null)
            {
                _context.EventRatings.Remove(rating);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(ManageRatings));
        }

        // ========== DOANH THU PHIM ==========
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult EventRevenue()
        {
            var eventRevenues = _context.Events
                .Select(e => new EventRevenueViewModel
                {
                    EventId = e.Id,
                    Title = e.Title,
                    TicketPrice = e.TicketPrice,
                    CurrentParticipants = _context.Registrations.Count(r => r.EventId == e.Id)
                })
                .AsEnumerable()
                .OrderByDescending(e => e.TotalRevenue)
                .ToList();

            return View(eventRevenues);
        }

        public IActionResult BackToDashboard() =>
            RedirectToAction(nameof(Index));
    }
}
