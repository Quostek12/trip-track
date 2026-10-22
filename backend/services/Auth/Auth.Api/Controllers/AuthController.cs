using System.Security.Claims;
using Auth.Api.Context;
using Auth.Api.DTOs;
using Auth.Api.Models;
using Auth.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Auth.Api.Controllers;

[ApiController]
[Route("")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<UserRole> _roleManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly AuthDbContext _context;

    public AuthController(
        UserManager<User> userManager,
        RoleManager<UserRole> roleManager,
        SignInManager<User> signInManager,
        IJwtTokenService jwtTokenService,
        AuthDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _context = context;
        _roleManager = roleManager;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenDTOResponse>> Login([FromBody] LoginDTO dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        
        if (user is null or{IsActive: false})
        {
            return Unauthorized("Niepoprawne dane logowania lub konto jest nieaktywne!");
        } else if (User.Identity is { IsAuthenticated: true })
        {
            return BadRequest("Użytkownik już jest zalogowany!");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            return StatusCode(StatusCodes.Status423Locked, "Konto zostało tymczasowo zablokowane!");
        }

        if (!result.Succeeded)
        {
            return Unauthorized("Niepoprawne adres email lub hasło.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles, out var jti);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenValue,
            JwtId = jti,
            UserId = user.Id,
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(3),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshTokenEntity);

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync();

        return Ok(new TokenDTOResponse(accessToken, refreshTokenValue));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenDTOResponse>> RefreshToken([FromBody] TokenDTORequest dto)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
        if (principal is null) return BadRequest("Nieprawidłowy AccessToken!");

        var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var userIdString = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(jti) || !Guid.TryParse(userIdString, out var userId))
        {
            return BadRequest("Nieprawidłowa struktura tokena!");
        }

        var storedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == dto.RefreshToken);

        if (storedRefreshToken == null) return NotFound("Podany refreshToken nie istnieje!");
        if (storedRefreshToken.IsUsed || storedRefreshToken.IsRevoked) return Unauthorized("Token jest zużyty!");
        if (storedRefreshToken.Expires < DateTime.UtcNow)
        {
            return Unauthorized("RefreshToken wygasł, zaloguj się ponownie");
        }

        if (storedRefreshToken.JwtId != jti || storedRefreshToken.UserId != userId)
        {
            return Unauthorized("Podany token nie pasuje do dostarczonego Access tokena!");
        }

        storedRefreshToken.IsUsed = true;
        _context.RefreshTokens.Update(storedRefreshToken);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive)
        {
            return Unauthorized("Użytkownik nie istnieje lub jest nieaktywny!");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles, out var newJti);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = newRefreshToken,
            JwtId = newJti,
            UserId = user.Id,
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(3),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        return Ok(new TokenDTOResponse(newAccessToken, newRefreshToken));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] TokenDTORequest dto)
    {
        var storedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == dto.RefreshToken);
        if (storedRefreshToken is {IsRevoked: false})
        {
            storedRefreshToken.IsRevoked = true;
            _context.RefreshTokens.Update(storedRefreshToken);
            await _context.SaveChangesAsync();
        } else
        {
            return BadRequest("Użytkownik już jest wylogowany!");
        }

        return Ok(new { Message = "Wylogowano pomyślnie" });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenDTOResponse>> Register([FromBody] RegisterDTO dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null) return BadRequest("Użytkownik z podanym email już istnieje!");

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                Message = "Nie udało się zarejestrować użytkownika!",
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        const string defaultRole = "Technician";

        if (await _roleManager.RoleExistsAsync(defaultRole))
        {
            await _userManager.AddToRoleAsync(user, defaultRole);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles, out var newJti);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = newRefreshToken,
            JwtId = newJti,
            UserId = user.Id,
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(3),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        return Ok(new TokenDTOResponse(newAccessToken, newRefreshToken));
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDTOResponse>> GetMe()
    {

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId)) return Unauthorized("Nie podano Id!");
        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user is null) return NotFound("Nie ma takiego użytkownika!");
        return Ok(new UserDTOResponse
        (
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            await _userManager.GetRolesAsync(user),
            user.CreatedAt,
            user.LastLoginAt
        ));
    }
}