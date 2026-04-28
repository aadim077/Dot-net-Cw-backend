using Microsoft.AspNetCore.Identity;
using VehicleParts.Application.DTOs.Auth;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;
using VehicleParts.Domain.Enums;

namespace VehicleParts.Infrastructure.Services;

internal sealed class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(UserManager<AppUser> userManager, IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "User with this email already exists."
            };
        }

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new AuthResponse
            {
                IsSuccess = false,
                Message = $"Registration failed: {errors}"
            };
        }

        var role = string.IsNullOrEmpty(request.Role) ? UserRoles.Customer : request.Role;
        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            return new AuthResponse
            {
                IsSuccess = false,
                Message = $"Failed to assign role: {errors}"
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiry) = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new AuthResponse
        {
            IsSuccess = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = expiry,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = role,
            Message = "Registration successful."
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "Invalid email or password."
            };
        }

        if (!user.IsActive)
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "User account is inactive."
            };
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "Invalid email or password."
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiry) = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new AuthResponse
        {
            IsSuccess = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = expiry,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? string.Empty,
            Message = "Login successful."
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "Invalid token."
            };
        }

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "Invalid token claims."
            };
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.RefreshToken != request.RefreshToken)
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "Invalid refresh token."
            };
        }

        if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "Refresh token has expired."
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (newAccessToken, expiry) = _jwtTokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new AuthResponse
        {
            IsSuccess = true,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiry = expiry,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? string.Empty,
            Message = "Token refreshed successfully."
        };
    }

    public async Task<bool> RevokeTokenAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded;
    }
}
