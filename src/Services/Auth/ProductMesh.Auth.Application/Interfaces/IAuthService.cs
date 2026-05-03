using ProductMesh.Auth.Application.DTOs;
using ProductMesh.Shared.Wrappers;

namespace ProductMesh.Auth.Application.Interfaces;

public interface IAuthService
{
    Task<Result<TokenResponse>> LoginAsync(LoginRequest request);
    Task<Result<UserDto>> RegisterAsync(RegisterRequest request);
    Task<Result<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<Result> RevokeTokenAsync(RevokeTokenRequest request);
}
