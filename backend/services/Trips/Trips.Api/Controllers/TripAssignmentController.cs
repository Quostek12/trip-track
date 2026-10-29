using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trips.Api.Context;
using Trips.Api.DTOs;
using Trips.Api.Models;
using Worker.Contracts;

namespace Trips.Api.Controllers;

[Route("assignments")]
[ApiController]
public class TripAssignmentController : ControllerBase
{
    private readonly TripsDbContext _context;
    private readonly WorkerLookup.WorkerLookupClient _workerLookupClient;
    

    public TripAssignmentController(TripsDbContext context, WorkerLookup.WorkerLookupClient workerLookupClient)
    {
        _context = context;
        _workerLookupClient = workerLookupClient;
    }

    [HttpGet]
    public ActionResult<List<TripAssignmentDTOResponse>> GetAll([FromQuery] Guid? tripId)
    {
        var query = _context.TripAssignments.Include(a => a.Trip).AsQueryable();
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
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<List<TripAssignmentDTOResponse>>> GetById([FromRoute] Guid tripAssignmentId)
    {
        var query = await _context.TripAssignments.Include(a => a.Trip).Where(ta => ta.Id == tripAssignmentId).FirstOrDefaultAsync();
        if (query is null) return NotFound("Nie znaleiono takiego przypisania!"); 
        
        return Ok( new TripAssignmentDTOResponse
        {
            Id = query.Id,
            TripId = query.TripId,
            Trip = new TripSummaryDTO
            {
                Id = query.Trip.Id,
                TripNumber = query.Trip.TripNumber,
                ClientName = query.Trip.ClientName,
                Title = query.Trip.Title,
                Status = query.Trip.Status,
            },
            WorkerId = query.WorkerId,
            AssignedAt = query.AssignedAt,
        });
    }
    
    

    [HttpPost]
    public async Task<ActionResult<TripAssignmentDTOResponse>> AddTripDestination([FromBody] TripAssignmentDTOCreate dto)
    {
        var tripQuery = await _context.Trips.FindAsync(dto.TripId);
        if (tripQuery == null) return NotFound("Nie ma takiego wyjazdu!");

        var userQuery = await _workerLookupClient.ExistsAsync(new WorkerExistsRequest
        {
            WorkerId = dto.WorkerId.ToString()
        });

        if (userQuery is { Exists: false} or {IsActive: false}) return BadRequest("Taki użytkownik nie istnieje!");

        var tripAssignment = new TripAssignment
        {
            Id = Guid.NewGuid(),
            TripId = dto.TripId,
            WorkerId = dto.WorkerId,
            AssignedAt = DateTime.UtcNow,
        };
        
        _context.TripAssignments.Add(tripAssignment);
        await _context.SaveChangesAsync();
        
        return Ok(new TripAssignmentDTOResponse
        {
            Id = tripAssignment.Id,
            TripId = tripAssignment.TripId,
            Trip = new TripSummaryDTO
            {
                Id = tripQuery.Id,
                TripNumber = tripQuery.TripNumber,
                Title = tripQuery.Title,
                ClientName = tripQuery.ClientName,
                Status = tripQuery.Status
            },
            WorkerId = tripAssignment.WorkerId,
            AssignedAt = tripAssignment.AssignedAt
        });

    }
    
        [HttpDelete]
        public async Task<ActionResult> DeleteAssignment([FromQuery] Guid tripAssignmentId)
        {
            var query = await _context.TripAssignments.FindAsync(tripAssignmentId);

            if (query is null) return NotFound("Nie znaleziono takiego przypisania!");

            _context.TripAssignments.Remove(query);
            await _context.SaveChangesAsync();

            return NoContent();
        }
}