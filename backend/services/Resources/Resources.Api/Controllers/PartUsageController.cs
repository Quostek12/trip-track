using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resources.Api.Context;
using Resources.Api.DTOs;
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
    public async Task<ActionResult<IEnumerable<PartUsageDTOResponse>>> GetUsagePerTrip(Guid tripId)
    {
        return await _context.PartUsages.Where(p => p.TripId == tripId).Include(p => p.Resource).Select(p => new PartUsageDTOResponse
        {
            Id = p.Id,
            Resource = p.Resource,
            ResourceId = p.ResourceId,
            TripId = p.TripId,
            QuantityUsed = p.QuantityUsed,
            UsedAt = p.UsedAt,
        }).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<PartUsageDTOResponse>> RecordUsage(PartUsageDTORequest usage)
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

        var result = new PartUsage()
        {
            Id = Guid.NewGuid(),
            Resource = usage.Resource,
            ResourceId = usage.ResourceId,
            TripId = usage.TripId,
            QuantityUsed = usage.QuantityUsed,
            UsedAt = DateTime.UtcNow
        };

        _context.PartUsages.Add(result);
        await  _context.SaveChangesAsync();
        return Ok(result);
    }
}