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
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly AuthDbContext _context;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtTokenService jwtTokenService,
        AuthDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _context = context;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenDTOResponse>> Login([FromBody] LoginDTO dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !user.IsActive)
        {
            return Unauthorized("Niepoprawne dane logowania lub konto jest nieaktywne!");
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
    [AllowAnonymous]
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
        if (storedRefreshToken != null)
        {
            storedRefreshToken.IsRevoked = true;
            _context.RefreshTokens.Update(storedRefreshToken);
            await _context.SaveChangesAsync();
        }

        return Ok( new {Message = "Wylogowano pomyślnie"});
    }
} 