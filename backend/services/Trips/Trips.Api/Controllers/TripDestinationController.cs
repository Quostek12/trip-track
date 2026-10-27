using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trips.Api.Context;
using Trips.Api.DTOs;
using Trips.Api.Models;

namespace Trips.Api.Controllers;

[ApiController]
[Route("destination")]
public class TripDestinationController : ControllerBase
{
    private readonly TripsDbContext _context;
    public TripDestinationController(TripsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<TripDestinationDTOResponse>>> GetAll([FromQuery] Guid? tripId)
    {
        var query = _context.TripDestinations.Select(td => new TripDestinationDTOResponse
        {
            Trip = td.Trip,
            TripId = td.TripId,
            StopOrder = td.StopOrder,
            LocationName = td.LocationName,
            Address = td.Address,
            Latitude = td.Latitude,
            Longitude = td.Longitude,
            PlannedArrivalTime = td.PlannedArrivalTime,
            PlannedDepartureTime = td.PlannedDepartureTime,
            Notes = td.Notes
        }).AsQueryable();
        
        if(tripId.HasValue) query = query.Where(t => t.TripId == tripId);
        return await query.ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TripDestinationDTOResponse>> GetById([FromQuery] Guid id)
    {
        var query = await _context.TripDestinations.Where(t => t.Id == id).Select(td => new TripDestinationDTOResponse
        {
            Trip = td.Trip,
            TripId = td.TripId,
            StopOrder = td.StopOrder,
            LocationName = td.LocationName,
            Address = td.Address,
            Latitude = td.Latitude,
            Longitude = td.Longitude,
            PlannedArrivalTime = td.PlannedArrivalTime,
            PlannedDepartureTime = td.PlannedDepartureTime,
            Notes = td.Notes 
        }).FirstOrDefaultAsync();
        if (query == null) return NotFound("Nie ma celu wyjazdu o takim Id!");

        return query;
    }
    
    [HttpPost]
    public async Task<ActionResult<TripDestination>> CreateTripDestination(
        [FromBody] TripDestinationDTORequest requestDto)
    {
        var duplicateQuery = await _context.TripDestinations.Where(td =>
            td.TripId == requestDto.TripId && td.LocationName == requestDto.LocationName &&
            td.StopOrder == requestDto.StopOrder).FirstOrDefaultAsync();
        if (duplicateQuery != null) return BadRequest("Istnieje już taki cel podróży!");

        var newDestination = new TripDestination
        {
            Id = Guid.NewGuid(),
            Trip = requestDto.Trip,
            StopOrder = requestDto.StopOrder,
            LocationName = requestDto.LocationName,
            Address = requestDto.Address,
            PlannedArrivalTime = requestDto.PlannedArrivalTime,
            PlannedDepartureTime = requestDto.PlannedDepartureTime,
            Notes = requestDto.Notes
        };

        _context.TripDestinations.Add(newDestination);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new {id = newDestination.Id}, newDestination);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTripDestination([FromRoute] Guid id)
    {
        var query = await _context.TripDestinations.FindAsync(id);
        if (query == null) return NotFound("Nie znaleziono takiego celu podróży!");

        _context.TripDestinations.Remove(query);
        await _context.SaveChangesAsync();

        return NoContent
            ();
    }
}