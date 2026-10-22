namespace Auth.Api.DTOs;

public record RoleDTORequest (
    string Name,
    string Description
    );
    
public record RoleDTOResponse(
    Guid RoleId,
    string Name,
    string Description
    );
        
