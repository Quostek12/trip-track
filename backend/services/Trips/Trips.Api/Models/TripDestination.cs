namespace Trips.Api.Models;

public class TripDestination
{
    public Guid Id { get; set; }
    public Trip Trip { get; set; } = null!;
    public Guid TripId { get; set; }
    public int StopOrder { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    public DateTime PlannedArrivalTime { get; set; }
    public DateTime PlannedDepartureTime { get; set; }
    
    public string? Notes { get; set; }
}