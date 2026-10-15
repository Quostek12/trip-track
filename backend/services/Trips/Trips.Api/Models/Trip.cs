namespace Trips.Api.Models;

public enum TripStatus
{
    Planned,
    InTransit,
    OnSite,
    Completed,
    Cancelled
}

public class Trip
{
    public Guid Id { get; set; }
    public string TripNumber { get; set; } = string.Empty; // TRP/2026/10/14/001
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public TripStatus Status { get; set; } =  TripStatus.Planned;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

    public ICollection<TripDestination> Destinations = new List<TripDestination>();
    public ICollection<TripAssignment> Assignments = new List<TripAssignment>();
}