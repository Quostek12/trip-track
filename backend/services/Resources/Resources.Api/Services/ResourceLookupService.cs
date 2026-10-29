using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Contracts.ResourceLookup;
using Resources.Api.Context;

namespace Resources.Api.Services;

public sealed class ResourceLookupService : ResourceLookup.ResourceLookupBase
{
    private readonly ResourcesDbContext _context;

    public ResourceLookupService(ResourcesDbContext context)
    {
        _context = context;
    }

    public override async Task<ResourceExistsResponse> Exists(
        ResourceExistsRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.ResourceId, out var resourceId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "resource_id must be a GUID."));
        }

        var resource = await _context.Resources.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == resourceId, context.CancellationToken);

        return new ResourceExistsResponse
        {
            Exists = resource is not null,
            Status = resource is null
                ? ResourceStatus.Available
                : ToContractStatus(resource.Status)
        };
    }

    public override async Task<ResourceSummaryResponse> GetSummary(
        ResourceSummaryRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.ResourceId, out var resourceId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "resource_id must be a GUID."));
        }

        var resource = await _context.Resources.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == resourceId, context.CancellationToken);

        if (resource is null)
        {
            return new ResourceSummaryResponse { ResourceId = request.ResourceId };
        }

        return new ResourceSummaryResponse
        {
            ResourceId = resource.Id.ToString(),
            Name = resource.Name,
            Status = ToContractStatus(resource.Status),
            Type = ToContractType(resource.Type)
        };
    }

    private static ResourceStatus ToContractStatus(Models.ResourceStatus status) =>
        status switch
        {
            Models.ResourceStatus.InUse => ResourceStatus.InUse,
            Models.ResourceStatus.Consumable => ResourceStatus.Consumable,
            _ => ResourceStatus.Available
        };

    private static ResourceType ToContractType(Models.ResourceType type) =>
        type switch
        {
            Models.ResourceType.Consumable => ResourceType.Consumable,
            Models.ResourceType.Car => ResourceType.Car,
            _ => ResourceType.Tool
        };
}
