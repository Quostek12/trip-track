using Microsoft.EntityFrameworkCore;
using Trips.Api.Models;
using Trips.Api.Context;
using Shared.Extensions;

namespace Trips.Api.Utils;

public class TripNumberGenerator
{
    public static string GenerateTripNumber(TripsDbContext dbContext)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var entry = dbContext.TripNumberHelpers.SingleOrDefault(e => e.Date == today);
        int number;

        if (entry == null)
        {
            var newEntry = new TripNumberHelper
            {
                Date = today,
                Value = 1
            };
            dbContext.TripNumberHelpers.Add(newEntry);
            number = newEntry.Value;
        }
        else
        {
            entry.Value++;
            number = entry.Value;
        }

        try
        {
            dbContext.SaveChanges();
        }
        catch (DbUpdateException)
        {
            throw new BadHttpRequestException("Coś poszło nie tak z inkrementowaniem!");
        }

        return today.ToString().ToTripString(number.ToString()).Replace('.', '/');
    }
}