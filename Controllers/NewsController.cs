using EventManager.Models;
using Microsoft.AspNetCore.Mvc;
using System;

[ApiController]
[Route("api/[controller]")]
public class ApiEventController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ApiEventController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetEvents()
    {
        var events = _context.Events.ToList();
        return Ok(events);
    }

    [HttpGet("{id}")]
    public IActionResult GetEvent(int id)
    {
        var evt = _context.Events.FirstOrDefault(e => e.Id == id);
        if (evt == null)
            return NotFound();
        return Ok(evt);
    }
}
