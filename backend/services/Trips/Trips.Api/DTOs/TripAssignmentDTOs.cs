using Trips.Api.Models;

namespace Trips.Api.DTOs;

public class TripAssignmentDTOCreate
{
    public Guid TripId { get; set; }
    public Trip Trip { get; set; } = null!;
    
    public Guid WorkerId { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class TripAssignmentDTOResponse
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public Trip Trip { get; set; } = null!;
    
    public Guid WorkerId { get; set; }
    public DateTime AssignedAt { get; set; }
}