using Trips.Api.Models;

namespace Trips.Api.DTOs;

public class TripDTORequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public ICollection<TripDestination> Destinations = new List<TripDestination>();
    public ICollection<TripAssignment> Assignments = new List<TripAssignment>();
}

public class TripDTOResponse
{
    public Guid Id { get; set; }
    public string TripNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public TripStatus Status {get; set;}
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public ICollection<TripDestinationDTOResponse> Destinations = new List<TripDestinationDTOResponse>();
    public ICollection<TripAssignmentDTOResponse> Assignments = new List<TripAssignmentDTOResponse>();
}