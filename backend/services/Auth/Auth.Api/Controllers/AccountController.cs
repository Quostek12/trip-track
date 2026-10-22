using Auth.Api.Context;
using Auth.Api.DTOs;
using Auth.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Controllers;

public class AccountController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<UserRole> _roleManager;

    public AccountController(AuthDbContext context, UserManager<User> userManager,  RoleManager<UserRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDTOResponse>>> GetAll([FromQuery] string? userRole)
    {
        IList<User> query = new List<User>();
        if (userRole != null && _roleManager.RoleExistsAsync(userRole).Result)
        {
            query = await _userManager.GetUsersInRoleAsync(userRole);
        }
        else if (userRole != null)
        {
            return BadRequest("Podana rola nie istnieje!");
        }

        query = _userManager.Users.ToList();
        
        return Ok(query.Select(async u => new UserDTOResponse(u.Id, u.Email, u.FirstName, u.LastName, await _userManager.GetRolesAsync(u), u.CreatedAt, u.LastLoginAt)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDTOResponse>> GetById([FromRoute] Guid id)
    {
       var query =  await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
       if (query == null) return NotFound("Nie ma takiego użytkownika!");
       return Ok(new UserDTOResponse
       (
           query.Id,
           query.Email,
           query.FirstName,
           query.LastName,
           await _userManager.GetRolesAsync(query),
           query.CreatedAt,
           query.LastLoginAt
       ));
    }

    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<RoleDTOResponse>>> GetAllRoles()
    {
        var query =  await _roleManager.Roles.ToListAsync();
        return Ok(query.Select(r => new RoleDTOResponse(r.Id, r.Name ?? "Nieznana", r.Description ?? "")));
    }

    [HttpGet("roles/{id:guid}")]
    public async Task<ActionResult<RoleDTOResponse>> GetRoleById([FromRoute] Guid id)
    {
        var query = await _roleManager.Roles.Where(r => r.Id == id).FirstOrDefaultAsync();
        if (query == null) return NotFound("Nie ma takiej roli");
        return Ok(query);
    }

    [HttpPost("roles")]
    public async Task<ActionResult<RoleDTOResponse>> CreateRole([FromBody] RoleDTORequest requestDto)
    {
        var query = await  _roleManager.Roles.Where(r => r.Name == requestDto.Name)
            .FirstOrDefaultAsync();
        if (query != null) return BadRequest("Podana rola już istnieje!");

        UserRole newRole = new UserRole
        {
            Name = requestDto.Name,
            Description = requestDto.Description
        };
        var result = await _roleManager.CreateAsync(newRole);

        if (!result.Succeeded)
        {
            return BadRequest("Coś poszło nie tak przy dodawaniu roli!");
        }
        return Ok(new { newRole.Id, newRole.Name, newRole.Description});
    }

    [HttpDelete("roles/{id:guid}")]
    public async Task<IActionResult> DeleteRole([FromRoute] Guid id)
    {
        var query = _roleManager.FindByIdAsync(id.ToString()).Result;
        if (query is null) return NotFound("Nie znaleziono takiej roli!");
        var result = await _roleManager.DeleteAsync(query);
        
        if (!result.Succeeded) return BadRequest("Coś poszło nie tak przy usuwaniu roli!");
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult<UserDTOResponse>> UpdateUser([FromBody] UserDTORequest userRequest)
    {
        var query = await _userManager.FindByEmailAsync(userRequest.Email);
        if (query == null) return NotFound("Nie ma takiego użytkownika!");
        var result = await _userManager.UpdateAsync(query);

        if (!result.Succeeded) return BadRequest("Coś poszło nie tak przy zmianie użytkownika!");
        return Ok(await _userManager.FindByIdAsync(query.Id.ToString()));
    }
    
    //TODO: dodac usuwanie uzytkownika i przetestowac!!
}