using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Repository.Model;
using Service.DTOs;
using Service.Helpers;
using Service.Interface;

namespace Service.Implement;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;

    public AuthService(IUnitOfWork unitOfWork, IEmailService emailService, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _tokenService = tokenService;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto registerDto)
    {
        // Check if email already exists
        var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
        if (existingUser != null)
        {
            return (false, "Email is already registered");
        }

        // Check if there's a pending OTP for this email (not expired and not used)
        var existingOtp = await _unitOfWork.OtpCodes.FirstOrDefaultAsync(
            o => o.Email == registerDto.Email && !o.IsUsed && o.ExpiryDate > DateTime.UtcNow
        );

        if (existingOtp != null)
        {
            return (false, "An OTP has already been sent to this email. Please check your inbox or wait for it to expire.");
        }

        // Store registration data temporarily
        var pendingRegistration = new PendingRegistration
        {
            Email = registerDto.Email,
            Password = registerDto.Password,
            FullName = registerDto.FullName,
            Role = registerDto.Role,
            CreatedAt = DateTime.UtcNow
        };
        RegistrationCache.Add(registerDto.Email, pendingRegistration);

        // Generate OTP
        var otpCode = GenerateOtpCode();
        
        // Save OTP to database
        var otp = new OtpCode
        {
            Email = registerDto.Email,
            Code = otpCode,
            ExpiryDate = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow,
            IsUsed = false
        };

        await _unitOfWork.OtpCodes.AddAsync(otp);
        await _unitOfWork.SaveChangesAsync();

        // Send OTP via email
        try
        {
            await _emailService.SendOtpEmailAsync(registerDto.Email, otpCode);
        }
        catch (Exception ex)
        {
            return (false, $"Failed to send OTP email: {ex.Message}");
        }

        return (true, "OTP has been sent to your email. Please verify to complete registration.");
    }

    public async Task<(bool Success, string Message, AuthResponseDto? Data)> VerifyOtpAndCompleteRegistrationAsync(VerifyOtpDto verifyOtpDto)
    {
        // Find OTP
        var otp = await _unitOfWork.OtpCodes.FirstOrDefaultAsync(
            o => o.Email == verifyOtpDto.Email && 
                 o.Code == verifyOtpDto.Code && 
                 !o.IsUsed
        );

        if (otp == null)
        {
            return (false, "Invalid OTP code", null);
        }

        if (otp.ExpiryDate < DateTime.UtcNow)
        {
            return (false, "OTP has expired", null);
        }

        // Check if user already exists
        var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == verifyOtpDto.Email);
        if (existingUser != null)
        {
            return (false, "User already exists", null);
        }

        // Get registration data from cache
        var pendingReg = RegistrationCache.Get(verifyOtpDto.Email);
        if (pendingReg == null)
        {
            return (false, "Registration data expired. Please register again.", null);
        }

        // Mark OTP as used
        otp.IsUsed = true;
        _unitOfWork.OtpCodes.Update(otp);
        await _unitOfWork.SaveChangesAsync();

        // Complete registration
        var result = await CompleteRegistrationAsync(
            pendingReg.Email,
            pendingReg.Password,
            pendingReg.FullName,
            pendingReg.Role
        );

        // Remove from cache
        RegistrationCache.Remove(verifyOtpDto.Email);

        return result;
    }

    private async Task<(bool Success, string Message, AuthResponseDto? Data)> CompleteRegistrationAsync(
        string email, string password, string fullName, string role)
    {
        // Hash password
        var hashedPassword = HashPassword(password);

        // Create user
        var user = new User
        {
            Email = email,
            Password = hashedPassword,
            FullName = fullName,
            Role = role.ToLower(),
            IsBanned = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Create Student or Teacher profile
        if (role.ToLower() == "student")
        {
            var student = new Student
            {
                UserId = user.Id,
                StudentIdCode = GenerateStudentCode()
            };
            await _unitOfWork.Students.AddAsync(student);
        }
        else if (role.ToLower() == "teacher")
        {
            var teacher = new Teacher
            {
                UserId = user.Id,
                TeacherIdCode = GenerateTeacherCode()
            };
            await _unitOfWork.Teachers.AddAsync(teacher);
        }

        await _unitOfWork.SaveChangesAsync();

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        // Get user profile
        var userDto = await GetUserDtoAsync(user.Id);

        var authResponse = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = userDto!
        };

        return (true, "Registration completed successfully", authResponse);
    }

    public async Task<(bool Success, string Message, AuthResponseDto? Data)> LoginAsync(LoginDto loginDto)
    {
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null)
        {
            return (false, "Invalid email or password", null);
        }

        if (user.IsBanned == true)
        {
            return (false, "Your account has been banned", null);
        }

        if (!VerifyPassword(loginDto.Password, user.Password))
        {
            return (false, "Invalid email or password", null);
        }

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        // Get user profile
        var userDto = await GetUserDtoAsync(user.Id);

        var authResponse = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = userDto!
        };

        return (true, "Login successful", authResponse);
    }

    public async Task<(bool Success, string Message, AuthResponseDto? Data)> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _unitOfWork.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.Token == refreshToken && !rt.IsRevoked
        );

        if (storedToken == null)
        {
            return (false, "Invalid refresh token", null);
        }

        if (storedToken.ExpiryDate < DateTime.UtcNow)
        {
            return (false, "Refresh token has expired", null);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(storedToken.UserId);
        if (user == null || user.IsBanned == true)
        {
            return (false, "User not found or banned", null);
        }

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Revoke old refresh token
        storedToken.IsRevoked = true;
        _unitOfWork.RefreshTokens.Update(storedToken);

        // Save new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        // Get user profile
        var userDto = await GetUserDtoAsync(user.Id);

        var authResponse = new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = userDto!
        };

        return (true, "Token refreshed successfully", authResponse);
    }

    public async Task<(bool Success, string Message)> LogoutAsync(int userId)
    {
        var refreshTokens = await _unitOfWork.RefreshTokens.FindAsync(
            rt => rt.UserId == userId && !rt.IsRevoked
        );

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
            _unitOfWork.RefreshTokens.Update(token);
        }

        await _unitOfWork.SaveChangesAsync();

        return (true, "Logged out successfully");
    }

    private async Task<UserDto?> GetUserDtoAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return null;

        var userDto = new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };

        if (user.Role == "student")
        {
            var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            userDto.StudentIdCode = student?.StudentIdCode;
        }
        else if (user.Role == "teacher")
        {
            var teacher = await _unitOfWork.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            userDto.TeacherIdCode = teacher?.TeacherIdCode;
        }

        return userDto;
    }

    private string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private string GenerateStudentCode()
    {
        return $"STU{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
    }

    private string GenerateTeacherCode()
    {
        return $"TCH{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == hashedPassword;
    }
}
