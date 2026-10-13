using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resources.Api.Context;
using Resources.Api.Models;

namespace Resources.Api.Controllers;

[ApiController]
[Route("partUsage")]
public class PartUsageController : ControllerBase
{
    private readonly ResourcesDbContext _context;

    public PartUsageController(ResourcesDbContext context)
    {
        _context = context;
    }

    [HttpGet("trip/{tripId}")]
    public async Task<ActionResult<IEnumerable<PartUsage>>> GetUsagePerTrip(Guid tripId)
    {
        return await _context.PartUsages.Include(p => p.Resource).Where(p => p.TripId == tripId).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<PartUsage>> RecordUsage(PartUsage usage)
    {
        var resource = await _context.Resources.FindAsync(usage.ResourceId);
        if (resource == null)
        {
            return NotFound("Podany zasób nie istnieje!");
        }

        if (resource.Type != ResourceType.Consumable)
        {
            return BadRequest("Ten typ nie jest zużywalny!");
        }

        if (resource.Quantity < usage.QuantityUsed)
        {
            return BadRequest("Zużyto więcej niż jest na stanie!");
        }

        resource.Quantity -= usage.QuantityUsed;
        usage.Id = Guid.NewGuid();
        usage.UsedAt = DateTime.UtcNow;

        _context.PartUsages.Add(usage);
        await  _context.SaveChangesAsync();
        return Ok(usage);
    }
}