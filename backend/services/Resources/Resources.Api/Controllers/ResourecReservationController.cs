using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resources.Api.Context;
using Resources.Api.DTOs;
using Resources.Api.Models;

namespace Resources.Api.Controllers;

[ApiController]
[Route("reservation")]
public class ResourecReservationController : ControllerBase
{
    private readonly ResourcesDbContext _context;

    public ResourecReservationController(ResourcesDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets resource reservations, optionally filtered by status.
    /// </summary>
    /// <param name="status">The optional reservation status filter.</param>
    /// <returns>A collection of matching reservations.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResourceReservationDTOResponse>>> GetReservations(
        [FromQuery] ReservationStatus? status)
    {
        var query = _context.ResourceReservations.Select(r => new ResourceReservationDTOResponse
        {
            Id = r.Id,
            ResourceId = r.ResourceId,
            Resource = r.Resource,
            TripId = r.TripId,
            ReservedByWorker = r.ReservedByWorker,
            Status = r.Status,
            StartAt = r.StartAt,
            EndAt = r.EndAt,
            Quantity = r.Quantity,
        }).AsQueryable();

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
    public async Task<ActionResult<ResourceReservationDTOResponse>> GetReservationById([FromRoute] Guid id)
    {
        var reservation = await _context.ResourceReservations.Where(r => r.Id == id).Select(r => new ResourceReservationDTOResponse
        {
            Id = r.Id,
            ResourceId = r.ResourceId,
            Resource = r.Resource,
            TripId = r.TripId,
            ReservedByWorker = r.ReservedByWorker,
            Status = r.Status,
            StartAt = r.StartAt,
            EndAt = r.EndAt,
            Quantity = r.Quantity,
        }).FirstOrDefaultAsync();

        if (reservation == null)
        {
            return NotFound("Nie ma takiej rezerwacji!");
        }

        return reservation;
    }

    [HttpPost]
    public async Task<ActionResult> CreateReservation([FromBody] ResourceReservationDTORequest resource)
    {
        var dbResource = await _context.Resources.FindAsync(resource.ResourceId);
        if (dbResource == null)
        {
            return NotFound("Podany zasób nie istnieje!");
        }

        var newReservation = new ResourceReservation
        {
            Id = Guid.NewGuid(),
            ResourceId = resource.ResourceId,
            Resource = resource.Resource,
            TripId = resource.TripId,
            ReservedByWorker = Guid.NewGuid(), // TO BE CHANGED!!
            Status = ReservationStatus.Reserved,
            StartAt = resource.StartAt,
            EndAt = resource.EndAt,
            Quantity = resource.Quantity,
        };
        
        _context.ResourceReservations.Add(newReservation);
        await _context.SaveChangesAsync();
        
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