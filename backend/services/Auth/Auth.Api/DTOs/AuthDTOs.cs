namespace Auth.Api.DTOs;

public record TokenDTOResponse(string AccessToken, string RefreshToken); // acc OUT, ref OUT

public record TokenDTORequest(string AccessToken, string RefreshToken); // acc IN, ref IN

public record LoginDTO(string Email, string Password);

public record RegisterDTO(string Email, string Password, string FirstName, string LastName);