namespace Trips.Api.Models;

public class TripAssignment
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public Trip Trip { get; set; } = null!;
    
    public Guid WorkerId { get; set; }
    public DateTime AssignedAt { get; set; }
}