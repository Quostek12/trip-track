using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Trips.Api.Context;
using Trips.Api.DTOs;

namespace Trips.Api.Controllers;

[Route("assignments")]
[ApiController]
public class TripAssignmentController : ControllerBase
{
    private readonly TripsDbContext _context;
    

    public TripAssignmentController(TripsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<TripAssignmentDTOResponse>>> GetAll([FromQuery] Guid? tripId)
    {
        var query = _context.TripAssignments.AsQueryable();
        if (tripId != null)
        {
            query = query.Where(t => t.TripId == tripId);
        }

        return Ok(query.Select(ta => new TripAssignmentDTOResponse
        {
            Id = ta.Id,
            TripId = ta.TripId,
            Trip = new TripSummaryDTO
            {
                Id = ta.Trip.Id,
                TripNumber = ta.Trip.TripNumber,
                ClientName = ta.Trip.ClientName,
                Title = ta.Trip.Title,
                Status = ta.Trip.Status,
            },
            WorkerId = ta.WorkerId,
            AssignedAt = ta.AssignedAt,
        }));
    }

    [HttpPost]
    public async Task<ActionResult<TripAssignmentDTOResponse>> AddTripDestination([FromBody] TripAssignmentDTOCreate dto)
    {
        var tripQuery = await _context.Trips.FindAsync(dto.TripId);
        if (tripQuery == null) return NotFound("Nie ma takiego wyjazdu!");
        
        // var userQuery = await 



        return Ok();
    }
}