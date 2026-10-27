namespace Workers.Api.Models;

public class WorkerSkill
{
    public Guid Id { get; set; }
    public Guid WorkerId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string LicenseNumber  { get; set; } = string.Empty;
    public DateTime ValidUntil { get; set; }
}