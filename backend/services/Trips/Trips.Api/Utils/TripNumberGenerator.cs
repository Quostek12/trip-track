using Trips.Api.Models;
using Trips.Api.Context;
using Shared.Extensions;

namespace Trips.Api.Utils;

public class TripNumberGenerator
{
    public static string GenerateTripNumber(TripsDbContext dbContext)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = dbContext.TripNumberHelpers.Where(e => e.Date == today).ToList();
        if (query.Count == 0)
        {
            var entry = new TripNumberHelper
            {
                Date = today,
                Value = 1
            };
            dbContext.TripNumberHelpers.Add(entry);
            dbContext.SaveChangesAsync();
            return today.ToString().ToTripString("1");
        }

        return query[0].Date.ToString().ToTripString(query[0].Value.ToString());
    }
}