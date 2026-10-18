using Resources.Api.Models;

namespace Resources.Api.DTOs;

public class ResourceDTORequest
{
    public required string Name { get; set; }
    public string? SerialNumber { get; set; }
    public required ResourceType Type { get; set; }
    public string Unit { get; set; } = "szt";
    public decimal Quantity { get; set; }

    public ICollection<ResourceReservation> Reservations { get; set; } = new List<ResourceReservation>();
    public ICollection<PartUsage> PartsUsages { get; set; } = new List<PartUsage>();
}

public class ResourceDTOResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? SerialNumber { get; set; }
    public required ResourceType Type { get; set; }
    public string Unit { get; set; } = "szt";
    public decimal Quantity { get; set; } 
    public ResourceStatus Status { get; set; } = ResourceStatus.Available;

    public ICollection<ResourceReservation> Reservations { get; set; } = new List<ResourceReservation>();
    public ICollection<PartUsage> PartsUsages { get; set; } = new List<PartUsage>();
}