using System.Security.Claims;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Application.Interfaces;

public interface IJwtTokenService
{
    (string token, DateTime expiry) GenerateAccessToken(AppUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
