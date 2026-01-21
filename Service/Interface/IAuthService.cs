using System.Threading.Tasks;
using Service.DTOs;

namespace Service.Interface;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterDto registerDto);
    Task<(bool Success, string Message, AuthResponseDto? Data)> VerifyOtpAndCompleteRegistrationAsync(VerifyOtpDto verifyOtpDto);
    Task<(bool Success, string Message, AuthResponseDto? Data)> LoginAsync(LoginDto loginDto);
    Task<(bool Success, string Message, AuthResponseDto? Data)> RefreshTokenAsync(string refreshToken);
    Task<(bool Success, string Message)> LogoutAsync(int userId);
}
