using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resources.Api.Context;
using Resources.Api.DTOs;
using Resources.Api.Models;

namespace Resources.Api.Controllers;

/// <summary>
/// Provides operations for managing resources.
/// </summary>
[ApiController]
[Route("")]
public class ResourceController : ControllerBase
{
    private readonly ResourcesDbContext _context;

    public ResourceController(ResourcesDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all resources, optionally filtered by resource type.
    /// </summary>
    /// <param name="type">The optional resource type used to filter the results.</param>
    /// <returns>A collection of matching resources.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResourceDTOResponse>>> GetAll([FromQuery] ResourceType? type)
    {
        var query = _context.Resources.Select(r => new ResourceDTOResponse
        {
            Id = r.Id,
            Name = r.Name,
            SerialNumber = r.SerialNumber,
            Type = r.Type,
            Status = r.Status,
            Unit = r.Unit,
            UnitPrice = r.UnitPrice,
            Quantity = r.Quantity,
            Reservations = r.Reservations,
            PartsUsages = r.PartsUsages
        });

        if (type.HasValue)
        {
            query = query.Where(r => r.Type == type.Value);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets a resource by its identifier.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <returns>The requested resource, or 404 if it does not exist.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResourceDTOResponse>> GetById([FromRoute] Guid id)
    {
        var query = await _context.Resources
            .Where(r => r.Id == id)
            .Select(r => new ResourceDTOResponse
            {
                Id = r.Id,
                Name = r.Name,
                SerialNumber = r.SerialNumber,
                Type = r.Type,
                Status = r.Status,
                Unit = r.Unit,
                UnitPrice = r.UnitPrice,
                Quantity = r.Quantity,
                Reservations = r.Reservations,
                PartsUsages = r.PartsUsages
            })
            .FirstOrDefaultAsync();

        if (query == null)
        {
            return NotFound();
        }

        return query;
    }

    /// <summary>
    /// Creates a new resource.
    /// </summary>
    /// <param name="resourceDto">The resource data to create.</param>
    /// <returns>The newly created resource.</returns>
    [HttpPost]
    public async Task<ActionResult<ResourceDTOResponse>> CreateResource([FromBody] ResourceDTORequest resourceDto)
    {
        if (resourceDto is {Type: ResourceType.Car, Quantity: > 1, SerialNumber: not null})
        {
            return BadRequest("Tylko jeden samochód może mieć taką tablice rejestracyjną!");
        }

        if (resourceDto.SerialNumber is not null &&
            await _context.Resources.AnyAsync(r => r.SerialNumber == resourceDto.SerialNumber))
        {
            return BadRequest("Numer seryjny musi być unikalny!");
        }

        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            Name = resourceDto.Name,
            SerialNumber = resourceDto.SerialNumber,
            Type = resourceDto.Type,
            Unit = resourceDto.Unit,
            UnitPrice = resourceDto.UnitPrice,
            Quantity = resourceDto.Quantity
        };

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = resource.Id }, ToResponse(resource));
    }

    /// <summary>
    /// Updates an existing resource.
    /// </summary>
    /// <param name="id">The identifier of the resource to update.</param>
    /// <param name="resourceDto">The updated resource data.</param>
    /// <returns>No content when the resource is updated successfully.</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateResource([FromRoute] Guid id, [FromBody] ResourceDTORequest resourceDto)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource is null) return NotFound();

        if (resourceDto is {Type: ResourceType.Car, Quantity: > 1, SerialNumber: not null})
        {
            return BadRequest("Tylko jeden samochód może mieć taką tablice rejestracyjną!");
        }

        if (resourceDto.SerialNumber is not null &&
            await _context.Resources.AnyAsync(r => r.Id != id && r.SerialNumber == resourceDto.SerialNumber))
        {
            return BadRequest("Numer seryjny musi być unikalny!");
        }

        resource.Name = resourceDto.Name;
        resource.SerialNumber = resourceDto.SerialNumber;
        resource.Type = resourceDto.Type;
        resource.Unit = resourceDto.Unit;
        resource.UnitPrice = resourceDto.UnitPrice;
        resource.Quantity = resourceDto.Quantity;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Resources.AnyAsync(r => r.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    private static ResourceDTOResponse ToResponse(Resource resource) => new()
    {
        Id = resource.Id,
        Name = resource.Name,
        SerialNumber = resource.SerialNumber,
        Type = resource.Type,
        Status = resource.Status,
        Unit = resource.Unit,
        UnitPrice = resource.UnitPrice,
        Quantity = resource.Quantity,
        Reservations = resource.Reservations,
        PartsUsages = resource.PartsUsages
    };

    /// <summary>
    /// Deletes a resource.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <returns>No content when the resource is deleted successfully.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteResource([FromRoute] Guid id)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null)
        {
            return NotFound();
        }

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}