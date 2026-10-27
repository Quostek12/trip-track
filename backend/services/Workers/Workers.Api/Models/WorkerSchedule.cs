namespace Workers.Api.Models;

public class WorkerSchedule
{
    public Guid Id { get; set; }
    public Guid WorkerId { get; set; }
    public DateTime Date { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public ScheduleType Type { get; set; }
    public AbsenceType? AbsenceType { get; set; }
}

public enum ScheduleType
{
    Shift,
    Duty,
    Overtime
}

public enum AbsenceType
{
    Vacation,
    SickLeave,
    Training
}