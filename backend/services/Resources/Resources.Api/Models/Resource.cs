namespace Resources.Api.Models;

public enum ResourceType
{
    Tool,
    Consumable,
    Car
}

public enum ResourceStatus
{
    Available,
    InUse,
    Consumable
}

public class Resource
{
   public required Guid Id { get; set; }
   public required string Name { get; set; }
   public string? SerialNumber { get; set; }
   public required ResourceType Type { get; set; }
   public ResourceStatus Status { get; set; }
   public string Unit { get; set; } = "szt";
   public decimal Quantity { get; set; }

   public ICollection<ResourceReservation> Reservations { get; set; } = new List<ResourceReservation>();
   public ICollection<PartUsage> PartsUsages { get; set; } = new List<PartUsage>();
}