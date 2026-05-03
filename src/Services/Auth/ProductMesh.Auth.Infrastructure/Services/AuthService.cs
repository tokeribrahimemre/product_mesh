using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductMesh.Auth.Application.DTOs;
using ProductMesh.Auth.Application.Interfaces;
using ProductMesh.Auth.Domain.Entities;
using ProductMesh.Auth.Infrastructure.Identity;
using ProductMesh.Auth.Infrastructure.Persistence;
using ProductMesh.Shared.Wrappers;

namespace ProductMesh.Auth.Infrastructure.Services;

// AuthService implements the IAuthService interface, handling user registration, login,
// token refresh and revocation. Uses SOLID's Single Responsibility — this class only
// handles authentication orchestration, delegating JWT concerns to IJwtTokenService.
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly AuthDbContext _context;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        AuthDbContext context,
        IValidator<LoginRequest> loginValidator,
        IValidator<RegisterRequest> registerValidator,
        IValidator<RefreshTokenRequest> refreshValidator)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _context = context;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
        _refreshValidator = refreshValidator;
    }

    public async Task<Result<TokenResponse>> LoginAsync(LoginRequest request)
    {
        var validation = await _loginValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return Result<TokenResponse>.Failure(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Result<TokenResponse>.Failure("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email!, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return Result<TokenResponse>.Success(
            new TokenResponse(accessToken, refreshToken, refreshTokenEntity.ExpiresAt));
    }

    public async Task<Result<UserDto>> RegisterAsync(RegisterRequest request)
    {
        var validation = await _registerValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return Result<UserDto>.Failure(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Result<UserDto>.Failure("User with this email already exists.");

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Result<UserDto>.Failure(result.Errors.Select(e => e.Description).ToList());

        await _userManager.AddToRoleAsync(user, AppRoles.User);
        var roles = await _userManager.GetRolesAsync(user);

        return Result<UserDto>.Success(
            new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, roles));
    }

    public async Task<Result<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var validation = await _refreshValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return Result<TokenResponse>.Failure(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var claims = _jwtTokenService.ValidateExpiredToken(request.AccessToken);
        if (claims is null)
            return Result<TokenResponse>.Failure("Invalid access token.");

        var (userId, email) = claims.Value;

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && t.UserId == userId);

        if (storedToken is null || !storedToken.IsActive)
            return Result<TokenResponse>.Failure("Invalid or expired refresh token.");

        // Revoke old token and issue new one (token rotation)
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<TokenResponse>.Failure("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtTokenService.GenerateAccessToken(userId, email, roles);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        storedToken.ReplacedByToken = newRefreshToken;

        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = newRefreshToken,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        return Result<TokenResponse>.Success(
            new TokenResponse(newAccessToken, newRefreshToken, newRefreshTokenEntity.ExpiresAt));
    }

    public async Task<Result> RevokeTokenAsync(RevokeTokenRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

        if (storedToken is null || !storedToken.IsActive)
            return Result.Failure("Token not found or already revoked.");

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Result.Success("Token revoked successfully.");
    }
}
