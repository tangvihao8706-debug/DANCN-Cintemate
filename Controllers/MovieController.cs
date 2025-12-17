using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventManager.Data;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovieController(ApplicationDbContext context)
        {
            _context = context;
        }

        // anyone can view list
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .OrderByDescending(m => m.IsHot)
                .ThenByDescending(m => m.Rating)
                .ThenByDescending(m => m.ReleaseDate)
                .ToListAsync();

            return View(movies);
        }

        // anyone can view details
        public async Task<IActionResult> Details(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();
            return View(movie);
        }

        // Only Admin/Staff can create
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie)
        {
            if (ModelState.IsValid)
            {
                movie.CreatedBy = User?.Identity?.Name ?? "system";
                movie.UpdatedAt = DateTime.UtcNow;
                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // Edit
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();
            return View(movie);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie)
        {
            if (id != movie.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    movie.UpdatedAt = DateTime.UtcNow;
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Movies.Any(e => e.Id == movie.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // Delete confirm
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();
            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
