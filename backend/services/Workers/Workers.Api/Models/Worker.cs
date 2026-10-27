namespace Workers.Api.Models;

public class Worker
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime HIredAt { get; set; }

    public ICollection<WorkerSkill> WorkerSkills { get; set; } = new List<WorkerSkill>();
}