namespace ProductMesh.Auth.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(string userId, string email, IList<string> roles);
    string GenerateRefreshToken();
    (string userId, string email)? ValidateExpiredToken(string token);
}
