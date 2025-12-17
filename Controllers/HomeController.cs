using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using EventManager.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventManager.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 📌 Trang chủ hiển thị danh sách phim / suất chiếu (có lọc thể loại, địa điểm, ngày)
        public IActionResult Index(int? genreId, string? location, DateTime? date)
        {
            // Load danh sách thể loại phim
            ViewBag.Genres = new SelectList(_context.Genres.ToList(), "Id", "Name");

            // Base query
            var eventsQuery = _context.Events
                .Include(e => e.Genre)
                .OrderByDescending(e => e.Date)
                .AsQueryable();

            // 🎯 Lọc theo thể loại phim
            if (genreId.HasValue && genreId.Value > 0)
            {
                eventsQuery = eventsQuery.Where(e => e.GenreId == genreId.Value);
            }

            // 🎯 Lọc theo rạp / địa điểm
            if (!string.IsNullOrWhiteSpace(location))
            {
                eventsQuery = eventsQuery.Where(e => e.Location.Contains(location));
            }

            // 🎯 Lọc theo ngày chiếu
            if (date.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.Date.Date == date.Value.Date);
            }

            // ✨ Chia thành 2 danh sách: phim nổi bật & phim khác
            var featuredEvents = eventsQuery.Where(e => e.IsFeatured).ToList();
            var otherEvents = eventsQuery.Where(e => !e.IsFeatured).ToList();

            // Truyền ra View
            ViewBag.FeaturedEvents = featuredEvents;
            return View(otherEvents); // phần không nổi bật truyền qua Model
        }

        // ✅ Thêm suất chiếu vào Google Calendar
        [Authorize]
        public async Task<IActionResult> AddToCalendar(int id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var evt = await _context.Events.FindAsync(id);
            if (evt == null)
                return NotFound();

            var calendarEvent = new
            {
                summary = evt.Title,
                location = evt.Location,
                description = evt.Description,
                start = new
                {
                    dateTime = evt.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                    timeZone = "Asia/Ho_Chi_Minh"
                },
                end = new
                {
                    dateTime = evt.Date.AddHours(3).ToString("yyyy-MM-ddTHH:mm:ss"),
                    timeZone = "Asia/Ho_Chi_Minh"
                }
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var content = new StringContent(JsonSerializer.Serialize(calendarEvent), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://www.googleapis.com/calendar/v3/calendars/primary/events", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = " Đã thêm lịch xem phim vào Google Calendar!";
            }
            else
            {
                TempData["ErrorMessage"] = "Thêm vào Google Calendar thất bại: " + await response.Content.ReadAsStringAsync();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Privacy() => View();

        // 🔍 Tìm kiếm phim
        [HttpGet]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ViewBag.Message = "⚠Vui lòng nhập từ khóa tìm kiếm.";
                return View(new List<EventModel>());
            }

            var matchedEvents = _context.Events
                .Include(e => e.Genre) // ✅ Đảm bảo có thể loại khi tìm kiếm
                .Where(e =>
                    e.Title.Contains(query) ||
                    e.Description.Contains(query) ||
                    e.Location.Contains(query))
                .OrderByDescending(e => e.Date)
                .ToList();

            ViewBag.Query = query;
            return View(matchedEvents);
        }
    }
}
