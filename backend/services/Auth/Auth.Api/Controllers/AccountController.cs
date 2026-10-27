using Auth.Api.Context;
using Auth.Api.DTOs;
using Auth.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Controllers;

[ApiController]
[Route("")]
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
        IList<User> users = new List<User>();
        if (userRole != null)
        {
            if (!await _roleManager.RoleExistsAsync(userRole)) return BadRequest("Podana rola nie istnieje!");
            users = await _userManager.GetUsersInRoleAsync(userRole);
        }
        else
        {
            users = await _userManager.Users.ToListAsync();
        }
        
        var result = users.Select((u) => new UserDTOResponse(u.Id, u.Email, u.FirstName, u.LastName,
            (from UserRole in _context.Set<IdentityUserRole<Guid>>()
                join role in _roleManager.Roles on UserRole.RoleId equals role.Id
                where UserRole.UserId == u.Id
                select role.Name).ToList(), u.CreatedAt, u.LastLoginAt));

        return Ok(result);
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
           (from UserRole in _context.Set<IdentityUserRole<Guid>>()
               join role in _roleManager.Roles on UserRole.RoleId equals role.Id
               where UserRole.UserId == query.Id
               select role.Name).ToList(),
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
        return Ok(new RoleDTOResponse(query.Id, query.Name ?? "Nieznana", query.Description ?? ""));
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
        var query = await _userManager.FindByIdAsync(userRequest.Id.ToString());
        if (query == null) return NotFound("Nie ma takiego użytkownika!");
        
        var currentRoles = await _userManager.GetRolesAsync(query);
        
        var result = await _userManager.UpdateAsync(query);

        if (!result.Succeeded) return BadRequest("Coś poszło nie tak przy zmianie użytkownika!");
        var user = await _userManager.FindByIdAsync(query.Id.ToString());
        if (user == null) return NotFound("Nie znaleziono uzytkownika po update!");
        return Ok(new UserDTOResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            (from UserRole in _context.Set<IdentityUserRole<Guid>>()
                join role in _roleManager.Roles on UserRole.RoleId equals role.Id
                where UserRole.UserId == user.Id
                select role.Name).ToList(),
            user.CreatedAt,
            user.LastLoginAt
        ));
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id) 
    {
        var query = await _userManager.FindByIdAsync(id.ToString());
        if (query == null) return NotFound("Nie ma takiego użytkownika!");
        var result = await _userManager.DeleteAsync(query);

        if (!result.Succeeded) return BadRequest("Wystąpil błąd podczas usuwania użytkownika!");
        return Ok();
    }

    [HttpPut("{id:guid}/roleChange")]
    public async Task<IActionResult> ChangeUserRoles([FromBody] List<string> roles, [FromRoute] Guid id)
    {
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                return BadRequest("Podana rola nie istnieje: " + role);
            }
        }
        
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound("Nie znaleziono takiego użytkownika!");
        
        var requestedRoles = new HashSet<string>(roles);
        var currentRoles = new HashSet<string>(await _userManager.GetRolesAsync(user));

       var rolesToAdd = requestedRoles.Except(currentRoles).ToList();
       var rolesToRemove = currentRoles.Except(requestedRoles).ToList();

       if (rolesToRemove.Any())
       {
           var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
           if (!removeResult.Succeeded) return BadRequest("Coś poszło nie tak przy usuwaniu roli!");
       }

       if (rolesToAdd.Any())
       {
            var result = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!result.Succeeded) return BadRequest("Coś poszło nie tak przy dodawaniu roli!");
       }

        return Ok("Pomyślnie zaktualizowano role");
    }
}