using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Interface;
using Repository.Model;
using Service.DTOs;
using Service.Interface;

namespace Service.Implement;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IEmailService emailService, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto registerDto)
    {
        // Check if email already exists
        var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
        if (existingUser != null)
        {
            return (false, "Email is already registered");
        }

        // Check if there's a pending verification for this email
        var existingToken = await _unitOfWork.EmailVerificationTokens.FirstOrDefaultAsync(
            t => t.Email == registerDto.Email && !t.IsVerified && t.ExpiryDate > DateTime.UtcNow
        );

        if (existingToken != null)
        {
            return (false, "A verification email has already been sent to this address. Please check your inbox.");
        }

        // Generate verification token
        var verificationToken = Guid.NewGuid().ToString("N");

        // Hash password
        var hashedPassword = HashPassword(registerDto.Password);

        // Save verification token to database
        var emailVerification = new EmailVerificationToken
        {
            Email = registerDto.Email,
            Token = verificationToken,
            FullName = registerDto.FullName,
            Password = hashedPassword,
            Role = registerDto.Role.ToLower(),
            ExpiryDate = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            IsVerified = false
        };

        await _unitOfWork.EmailVerificationTokens.AddAsync(emailVerification);
        await _unitOfWork.SaveChangesAsync();

        // Get base URL from configuration or request
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7259";
        var verificationLink = $"{baseUrl}/api/Auth/verify-email?token={verificationToken}";

        // Check if email sending is enabled
        var enableEmailSending = bool.TryParse(_configuration["EmailSettings:EnableEmailSending"], out var enabled) && enabled;

        if (enableEmailSending)
        {
            // Send verification email
            try
            {
                await _emailService.SendVerificationEmailAsync(registerDto.Email, verificationLink);
            }
            catch (Exception ex)
            {
                // Log error but don't fail registration
                Console.WriteLine($"Failed to send email: {ex.Message}");
                return (false, $"Failed to send verification email: {ex.Message}");
            }
            
            return (true, "Registration successful! Please check your email to verify your account.");
        }
        else
        {
            // Email sending disabled - return verification link for testing
            return (true, $"Registration successful! Use this link to verify: {verificationLink}");
        }
    }

    public async Task<(bool Success, string Message, AuthResponseDto? Data)> VerifyEmailAsync(string token)
    {
        // Find verification token
        var verificationToken = await _unitOfWork.EmailVerificationTokens.FirstOrDefaultAsync(
            t => t.Token == token && !t.IsVerified
        );

        if (verificationToken == null)
        {
            return (false, "Invalid or already used verification token", null);
        }

        if (verificationToken.ExpiryDate < DateTime.UtcNow)
        {
            return (false, "Verification token has expired. Please register again.", null);
        }

        // Check if user already exists (in case of race condition)
        var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == verificationToken.Email);
        if (existingUser != null)
        {
            return (false, "User already exists", null);
        }

        // Create user
        var user = new User
        {
            Email = verificationToken.Email,
            Password = verificationToken.Password,
            FullName = verificationToken.FullName,
            Role = verificationToken.Role,
            IsBanned = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Create Student or Teacher profile
        if (verificationToken.Role == "student")
        {
            var student = new Student
            {
                UserId = user.Id,
                StudentIdCode = GenerateStudentCode()
            };
            await _unitOfWork.Students.AddAsync(student);
        }
        else if (verificationToken.Role == "teacher")
        {
            var teacher = new Teacher
            {
                UserId = user.Id,
                TeacherIdCode = GenerateTeacherCode()
            };
            await _unitOfWork.Teachers.AddAsync(teacher);
        }

        // Mark token as verified
        verificationToken.IsVerified = true;
        _unitOfWork.EmailVerificationTokens.Update(verificationToken);

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
            ExpiresAt = DateTime.UtcNow.AddHours(5),  // ? C?p nh?t t? 15 phút lên 5 gi?
            User = userDto!
        };

        return (true, "Email verified successfully! Your account has been created.", authResponse);
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
            ExpiresAt = DateTime.UtcNow.AddHours(5),  // ? C?p nh?t t? 15 phút lên 5 gi?
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
            ExpiresAt = DateTime.UtcNow.AddHours(5),  // ? C?p nh?t t? 15 phút lên 5 gi?
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

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        return await GetUserDtoAsync(userId);
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

    private string GenerateStudentCode()
    {
        // Format: STU + yyMMddHHmm + random 3 digits = 3 + 10 + 3 = 16 characters
        return $"STU{DateTime.UtcNow:yyMMddHHmm}{new Random().Next(100, 999)}";
    }

    private string GenerateTeacherCode()
    {
        // Format: TCH + yyMMddHHmm + random 3 digits = 3 + 10 + 3 = 16 characters
        return $"TCH{DateTime.UtcNow:yyMMddHHmm}{new Random().Next(100, 999)}";
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

    public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        // Check if user exists
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email);
        if (user == null)
        {
            // Return success even if user doesn't exist to prevent email enumeration
            return (true, "If the email exists in our system, a password reset code will be sent.");
        }

        if (user.IsBanned == true)
        {
            return (false, "Your account has been banned. Please contact support.");
        }

        // Check if there's a recent unused code (rate limiting - prevent spam)
        var recentCode = await _unitOfWork.PasswordResetCodes.FirstOrDefaultAsync(
            c => c.Email == forgotPasswordDto.Email && 
                 !c.IsUsed && 
                 c.CreatedAt > DateTime.UtcNow.AddMinutes(-2)
        );

        if (recentCode != null)
        {
            return (false, "A password reset code was recently sent. Please wait 2 minutes before requesting a new one.");
        }

        // Generate 6-digit OTP code
        var otpCode = GenerateOtpCode();

        // Save to database
        var passwordResetCode = new PasswordResetCode
        {
            Email = forgotPasswordDto.Email,
            Code = otpCode,
            ExpiryDate = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow,
            IsUsed = false,
            AttemptCount = 0
        };

        await _unitOfWork.PasswordResetCodes.AddAsync(passwordResetCode);
        await _unitOfWork.SaveChangesAsync();

        // Check if email sending is enabled
        var enableEmailSending = bool.TryParse(_configuration["EmailSettings:EnableEmailSending"], out var enabled) && enabled;

        if (enableEmailSending)
        {
            try
            {
                await _emailService.SendPasswordResetOtpAsync(forgotPasswordDto.Email, otpCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send password reset email: {ex.Message}");
                return (false, $"Failed to send password reset email: {ex.Message}");
            }
            
            return (true, "Password reset code has been sent to your email.");
        }
        else
        {
            // Email sending disabled - return OTP code for testing
            return (true, $"Password reset code (testing): {otpCode}");
        }
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        // Find the password reset code
        var resetCode = await _unitOfWork.PasswordResetCodes.FirstOrDefaultAsync(
            c => c.Code == resetPasswordDto.OtpCode && !c.IsUsed
        );

        if (resetCode == null)
        {
            return (false, "Invalid or already used OTP code.");
        }

        // Check expiry
        if (resetCode.ExpiryDate < DateTime.UtcNow)
        {
            return (false, "OTP code has expired. Please request a new one.");
        }

        // Check attempt count (max 5 attempts)
        if (resetCode.AttemptCount >= 5)
        {
            resetCode.IsUsed = true;
            _unitOfWork.PasswordResetCodes.Update(resetCode);
            await _unitOfWork.SaveChangesAsync();
            return (false, "Too many failed attempts. Please request a new code.");
        }

        // Find user
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == resetCode.Email);
        if (user == null)
        {
            return (false, "User not found.");
        }

        if (user.IsBanned == true)
        {
            return (false, "Your account has been banned. Please contact support.");
        }

        // Update password
        user.Password = HashPassword(resetPasswordDto.NewPassword);
        _unitOfWork.Users.Update(user);

        // Mark code as used
        resetCode.IsUsed = true;
        _unitOfWork.PasswordResetCodes.Update(resetCode);

        // Revoke all existing refresh tokens for security
        var refreshTokens = await _unitOfWork.RefreshTokens.FindAsync(
            rt => rt.UserId == user.Id && !rt.IsRevoked
        );

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
            _unitOfWork.RefreshTokens.Update(token);
        }

        await _unitOfWork.SaveChangesAsync();

        return (true, "Password has been reset successfully. Please login with your new password.");
    }

    private string GenerateOtpCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var number = BitConverter.ToUInt32(bytes, 0) % 1000000;
        return number.ToString("D6");
    }
}
