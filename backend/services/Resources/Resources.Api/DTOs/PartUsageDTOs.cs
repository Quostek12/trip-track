using Resources.Api.Models;

namespace Resources.Api.DTOs;

public class PartUsageDTORequest
{
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
    
    public Guid TripId { get; set; }
    
    public decimal QuantityUsed { get; set; }
    public DateTime UsedAt { get; set; }
}

public class PartUsageDTOResponse
{
    public Guid Id { get; set; }
    
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
    
    public Guid TripId { get; set; }
    
    public decimal QuantityUsed { get; set; }
    public DateTime UsedAt { get; set; }
}