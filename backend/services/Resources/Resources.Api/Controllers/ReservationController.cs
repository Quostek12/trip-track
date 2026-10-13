using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resources.Api.Context;
using Resources.Api.Models;

namespace Resources.Api.Controllers;

[ApiController]
[Route("reservation")]
public class ReservationController : ControllerBase
{
    private readonly ResourcesDbContext _context;

    public ReservationController(ResourcesDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets resource reservations, optionally filtered by status.
    /// </summary>
    /// <param name="status">The optional reservation status filter.</param>
    /// <returns>A collection of matching reservations.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResourceReservation>>> GetReservations(
        [FromQuery] ReservationStatus? status)
    {
        var query = _context.ResourceReservations.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(res => res.Status == status);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets a resource reservation by its identifier.
    /// </summary>
    /// <param name="id">The reservation identifier.</param>
    /// <returns>The reservation, or 404 when it does not exist.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceReservation>> GetReservationById([FromRoute] Guid id)
    {
        var reservation = await _context.ResourceReservations.FindAsync(id);

        if (reservation == null)
        {
            return NotFound("Nie ma takiej rezerwacji!");
        }

        return reservation;
    }

    [HttpPost]
    public async Task<ActionResult<ResourceReservation>> CreateReservation([FromBody] ResourceReservation resource)
    {
        var dbResource = await _context.Resources.FindAsync(resource.ResourceId);
        if (dbResource == null)
        {
            return NotFound("Podany zasób nie istnieje!");
        }

        resource.Id =  Guid.NewGuid();
        resource.Status = ReservationStatus.Reserved;
        
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ResourceReservation>> UpdateStatus([FromBody] ReservationStatus status,
        [FromRoute] Guid id)
    {
        var reservation = await _context.ResourceReservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound("Nie ma takiej rezerwacji!");
        }

        reservation.Status = status;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}