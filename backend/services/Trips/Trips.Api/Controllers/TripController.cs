using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trips.Api.Context;
using Trips.Api.DTOs;
using Trips.Api.Models;
using Trips.Api.Utils;

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
    public async Task<ActionResult<List<TripDTOResponse>>> GetAll([FromQuery] TripStatus? 
        status)
    {
        var query = _context.Trips.Select(t=> new TripDTOResponse
        {
            Id = t.Id,
            Title = t.Title,
            TripNumber = t.TripNumber,
            Description = t.Description,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            ClientName = t.ClientName,
            Status = t.Status,
            Destinations = t.Destinations.Select(d => new TripDestinationDTOResponse
            { 
                Trip = new TripSummaryDTO
                {
                    Id = d.Trip.Id,
                    TripNumber = d.Trip.TripNumber,
                    Title = d.Trip.Title,
                    ClientName = d.Trip.ClientName,
                    Status = d.Trip.Status
                },
                TripId = d.TripId,
                Address  = d.Address,
                Latitude = d.Latitude,
                Longitude = d.Longitude,
                LocationName = d.LocationName,
                StopOrder = d.StopOrder,
                PlannedArrivalTime = d.PlannedArrivalTime,
                PlannedDepartureTime = d.PlannedDepartureTime,
                Notes = d.Notes
            }).ToList(),
            Assignments = t.Assignments.Select(a => new TripAssignmentDTOResponse
            {
                Id = a.Id,
                TripId = a.TripId,
                Trip = new TripSummaryDTO
                {
                    ClientName = a.Trip.ClientName,
                    Id = a.Trip.Id,
                    Title = a.Trip.Title,
                    TripNumber = a.Trip.TripNumber
                },
                WorkerId = a.WorkerId,
                AssignedAt = a.AssignedAt,
            }).ToList(),
        }).AsQueryable();
        if (status.HasValue) query = query.Where(t => t.Status == status.Value);
        return await query.ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TripDTOResponse>> GetById([FromRoute] Guid id)
    {
        var query = await _context.Trips.Select(t => new TripDTOResponse
        {
            Id = t.Id,
            Title = t.Title,
            TripNumber = t.TripNumber,
            Description = t.Description,
            ClientName = t.ClientName,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            Status = t.Status,
            Destinations = t.Destinations.Select(d => new TripDestinationDTOResponse
            { 
                Trip = new TripSummaryDTO
                {
                    Id = d.Trip.Id,
                    TripNumber = d.Trip.TripNumber,
                    Title = d.Trip.Title,
                    ClientName = d.Trip.ClientName,
                    Status = d.Trip.Status
                },
                TripId = d.TripId,
                Address  = d.Address,
                Latitude = d.Latitude,
                Longitude = d.Longitude,
                LocationName = d.LocationName,
                StopOrder = d.StopOrder,
                PlannedArrivalTime = d.PlannedArrivalTime,
                PlannedDepartureTime = d.PlannedDepartureTime,
                Notes = d.Notes
            }).ToList(),
            Assignments = t.Assignments.Select(a => new TripAssignmentDTOResponse
            {
                Id = a.TripId,
                TripId = a.TripId,
                Trip = new TripSummaryDTO
                {
                    ClientName = a.Trip.ClientName,
                    Id = a.Trip.Id,
                    Title = a.Trip.Title,
                    TripNumber = a.Trip.TripNumber
                },
                WorkerId = a.WorkerId,
                AssignedAt = a.AssignedAt,
            }).ToList(),
        }).AsQueryable().FirstOrDefaultAsync(t => t.Id == id);
        if (query == null) return NotFound("Nie znaleziono wyjazdu o takim ID!");
        return query;
    }

    [HttpPost]
    public async Task<ActionResult<Trip>> CreateTrip([FromBody] TripDTORequest tripDto)
    {
        if (await _context.Trips.FirstOrDefaultAsync(t => t.Title == tripDto.Title) != null)
        {
            return BadRequest("Istnieje już podróż o takim tytule");
        }

        var newTrip = new Trip
        {
            Id = Guid.NewGuid(),
            TripNumber = TripNumberGenerator.GenerateTripNumber(_context),
            Title = tripDto.Title,
            Description = tripDto.Description,
            StartDate = tripDto.StartDate,
            EndDate = tripDto.EndDate,
            ClientName = tripDto.ClientName,
            Status = TripStatus.Planned,
            Assignments = tripDto.Assignments,
            Destinations = tripDto.Destinations,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Trips.Add(newTrip);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetById), new { id = newTrip.Id }, newTrip);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTrip([FromRoute] Guid id, [FromBody] Trip trip)
    {
        var entry = await _context.Trips.FindAsync(id);
        if (entry is null) return NotFound("Nie znaleziono takiego wyjazdu!");
        if (trip.Id != Guid.Empty && trip.Id != id) return BadRequest("Id nie zgadza się!");

        entry.Title = trip.Title;
        entry.Description = trip.Description;
        entry.ClientName = trip.ClientName;
        entry.Status = trip.Status;
        entry.StartDate = trip.StartDate;
        entry.EndDate = trip.EndDate;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Trips.AnyAsync(r => r.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTrip([FromRoute] Guid id)
    {
        var trip = await _context.Trips.FindAsync(id);
        if (trip == null) return NotFound("Nie ma wyjazdu o takim Id!");
        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateTripStatus([FromRoute] Guid id, [FromQuery] TripStatus status)
    {
        var entry = await _context.Trips.FirstOrDefaultAsync(t => t.Id == id);
        if (entry == null) return NotFound("Nie znaleziono wyjazdu o takim Id!");
        entry.Status = status;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Trips.AnyAsync(t => t.Id == id)) return  NotFound("W bazie nie ma takiego rekordu!");
        }

        return NoContent();
    }
}