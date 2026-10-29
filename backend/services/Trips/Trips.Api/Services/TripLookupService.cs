using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Contracts.TripLookup;
using Trips.Api.Context;

namespace Trips.Api.Services;

public sealed class TripLookupService : TripLookup.TripLookupBase
{
    private readonly TripsDbContext _context;

    public TripLookupService(TripsDbContext context)
    {
        _context = context;
    }

    public override async Task<TripExistsResponse> Exists(
        TripExistsRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.TripId, out var tripId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "trip_id must be a GUID."));
        }

        var trip = await _context.Trips.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == tripId, context.CancellationToken);

        return new TripExistsResponse
        {
            Exists = trip is not null,
            Status = trip is null ? TripStatus.Unspecified : ToContractStatus(trip.Status)
        };
    }

    public override async Task<TripSummaryResponse> GetSummary(
        TripSummaryRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.TripId, out var tripId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "trip_id must be a GUID."));
        }

        var trip = await _context.Trips.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == tripId, context.CancellationToken);

        return trip is null
            ? new TripSummaryResponse()
            : new TripSummaryResponse
            {
                TripNumber = trip.TripNumber,
                Title = trip.Title,
                ClientName = trip.ClientName
            };
    }

    private static TripStatus ToContractStatus(Models.TripStatus status) =>
        status switch
        {
            Models.TripStatus.Planned => TripStatus.Planned,
            Models.TripStatus.InTransit => TripStatus.InTransit,
            Models.TripStatus.OnSite => TripStatus.OnSite,
            Models.TripStatus.Completed => TripStatus.Completed,
            Models.TripStatus.Cancelled => TripStatus.Cancelled,
            _ => TripStatus.Unspecified
        };
}
