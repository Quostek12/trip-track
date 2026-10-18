using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Models;

public class UserRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    
    public UserRole() : base() {}
    
    public UserRole(string roleName, string description) : base(roleName)
    {
        Description = description;
    }
}