using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Worker.Contracts;
using Workers.Api.Context;

namespace Workers.Api.Services;

public sealed class WorkerLookupService : WorkerLookup.WorkerLookupBase
{
    private readonly WorkersDbContext _context;
    public WorkerLookupService(WorkersDbContext context)
    {
        _context = context;
    }
    
    public override async Task<WorkerExistsResponse> Exists(WorkerExistsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.WorkerId, out var workerId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "worker_id must be a GUID."));
        }

        var worker = await _context.Workers.AsNoTracking()
            .FirstOrDefaultAsync(wrk => wrk.Id == workerId, context.CancellationToken);

        return new WorkerExistsResponse
        {
            Exists = worker != null ? true : false,
            IsActive = worker?.IsActive ?? false
        };
    }

    public override async Task<WorkerSummaryResponse> GetSummary(WorkerSummaryRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.WorkerId, out var workerId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "worker_id must be a GUID."));
        }
        
        var worker = await _context.Workers.AsNoTracking()
            .FirstOrDefaultAsync(wrk => wrk.Id == workerId, context.CancellationToken);

        return new WorkerSummaryResponse
        {
            WorkerId = worker?.Id.ToString()  ?? "",
            WorkerDisplayName = worker?.FirstName + " " + worker?.LastName,
            IsActive = worker?.IsActive ?? false
        };
    }
}