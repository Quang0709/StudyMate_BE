using System.Security.Claims;

namespace Service.Interface;

public interface ITokenService
{
    string GenerateAccessToken(int userId, string email, string role);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
