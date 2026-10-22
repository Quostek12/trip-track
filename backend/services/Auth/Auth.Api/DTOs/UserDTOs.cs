namespace Auth.Api.DTOs;

public record UserDTOResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    IEnumerable<string> Roles,
    DateTime? CreatedAt,
    DateTime? LastLoginAt
    );

public record UserDTORequest(
    string Email,
    string FirstName,
    string LastName,
    IEnumerable<string> Roles);

