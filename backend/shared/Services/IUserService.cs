namespace Shared.Services;

public interface IUserService
{
    Guid? UserId { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string roleName);
}