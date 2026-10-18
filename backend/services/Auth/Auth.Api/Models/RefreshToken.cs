namespace Auth.Api.Models;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Token { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty;
    
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public bool IsUsed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}