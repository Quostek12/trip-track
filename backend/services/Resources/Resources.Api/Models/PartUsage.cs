namespace Resources.Api.Models;

public class PartUsage
{
    public Guid Id { get; set; }
    
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
    
    public Guid TripId { get; set; }
    public decimal QuantityUsed { get; set; }
    public DateTime UsedAt { get; set; }
}

