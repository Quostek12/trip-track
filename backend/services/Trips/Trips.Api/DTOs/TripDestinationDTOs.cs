using Trips.Api.Models;

namespace Trips.Api.DTOs;

public class TripDestinationDTORequest
{
    public Trip Trip { get; set; } = null!;
    public Guid TripId { get; set; }
    public int StopOrder { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    
    public DateTime PlannedArrivalTime { get; set; }
    public DateTime PlannedDepartureTime { get; set; }
    
    public string? Notes { get; set; }
}

public class TripDestinationDTOResponse
{
    public Trip Trip { get; set; } = null!;
    public Guid TripId { get; set; }
    public int StopOrder { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    
    public DateTime PlannedArrivalTime { get; set; }
    public DateTime PlannedDepartureTime { get; set; }
    
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    public string? Notes { get; set; }
}