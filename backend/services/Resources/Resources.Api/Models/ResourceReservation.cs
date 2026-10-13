using System;

namespace Resources.Api.Models;

public enum ReservationStatus
{
    Reserved,
    PickedUp,
    Returned
}

public class ResourceReservation
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    
    public Resource Resource { get; set; } = null!;
    
    public Guid TripId { get; set; }
    public Guid ReservedByWorker { get; set; }
    
    public ReservationStatus Status { get; set; } =  ReservationStatus.Reserved;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int Quantity { get; set; } = 1;
}
