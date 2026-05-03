namespace ProductMesh.Auth.Application.DTOs;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);

public record RefreshTokenRequest(string AccessToken, string RefreshToken);

public record RevokeTokenRequest(string RefreshToken);

public record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public record UserDto(string Id, string Email, string FirstName, string LastName, IList<string> Roles);
