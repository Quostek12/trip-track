using Resources.Api.Models;

namespace Resources.Api.DTOs;

public class ResourceReservationDTORequest
{
    public Guid ResourceId { get; set; }
    public Guid TripId { get; set; }
    
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int Quantity { get; set; }
}

public class ResourceReservationDTOResponse
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public ResourceSummaryDTO Resource { get; set; } = null!;
    
    public Guid TripId { get; set; }
    public Guid ReservedByWorker { get; set; }
    
    public ReservationStatus Status { get; set; } =  ReservationStatus.Reserved;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int Quantity { get; set; }
}

public class ResourceReservationStatusChangeDTORequest
{
    public ReservationStatus Status { get; set; }
}