using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trips.Api.Context;
using Trips.Api.Models;

namespace Trips.Api.Controllers;

[ApiController]
[Route("")]
public class TripController : ControllerBase
{
    private readonly TripsDbContext _context;
    
    public TripController(TripsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Trip>>> GetAll([FromQuery] TripStatus? 
        status)
    {
        var query = _context.Trips.AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status);
        }

        return await query.ToListAsync();
    }
    
    [HttpGet("{id}")]
    
}