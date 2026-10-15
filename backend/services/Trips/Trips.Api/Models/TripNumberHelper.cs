namespace Trips.Api.Models;

public class TripNumberHelper
{
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public int Value { get; set; } = 0; 
}