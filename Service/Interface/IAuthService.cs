using System.Threading.Tasks;
using Service.DTOs;

namespace Service.Interface;

public interface IAuthService
{
    Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterAsync(RegisterDto registerDto);
    Task<(bool Success, string Message, AuthResponseDto? Data)> LoginAsync(LoginDto loginDto);
    Task<(bool Success, string Message, AuthResponseDto? Data)> RefreshTokenAsync(string refreshToken);
    Task<(bool Success, string Message)> LogoutAsync(int userId);
    Task<UserDto?> GetUserByIdAsync(int userId);
}
