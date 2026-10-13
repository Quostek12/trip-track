using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resources.Api.Context;
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
    public async Task<ActionResult<IEnumerable<Resource>>> GetAll([FromQuery] ResourceType? type)
    {
        var query = _context.Resources.AsQueryable();

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
    public async Task<ActionResult<Resource>> GetById([FromRoute] Guid id)
    {
        var query = await _context.Resources.FindAsync(id);

        if (query == null)
        {
            return NotFound();
        }

        return query;
    }

    /// <summary>
    /// Creates a new resource.
    /// </summary>
    /// <param name="resource">The resource to create.</param>
    /// <returns>The newly created resource.</returns>
    [HttpPost]
    public async Task<ActionResult<Resource>> CreateResource([FromBody] Resource resource)
    {
        resource.Id = Guid.NewGuid();
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = resource.Id }, resource);
    }

    /// <summary>
    /// Updates an existing resource.
    /// </summary>
    /// <param name="id">The identifier of the resource to update.</param>
    /// <param name="resource">The updated resource data.</param>
    /// <returns>No content when the resource is updated successfully.</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateResource([FromRoute] Guid id, [FromBody] Resource resource)
    {
        if (id != resource.Id)
        {
            return BadRequest("Id nie zgadza się!");
        }

        _context.Entry(resource).State = EntityState.Modified;

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
        }

        return NoContent();
    }

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