using Microsoft.AspNetCore.Mvc;
using EventManager.Data;

namespace EventManager.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuggestionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SuggestionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<object>());

            var suggestions = _context.Events
                .Where(e => e.Title.Contains(query) || e.Location.Contains(query))
                .OrderByDescending(e => e.Date)
                .Take(5)
                .Select(e => new
                {
                    e.Id,
                    e.Title
                })
                .ToList();

            return Ok(suggestions);
        }
    }
}
