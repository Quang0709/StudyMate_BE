using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interface;
using System.Security.Claims;

namespace StudyMate_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new student account
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, message) = await _authService.RegisterAsync(registerDto);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { message });
    }

    /// <summary>
    /// Verify email address with token from email link
    /// </summary>
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Verification token is required" });
        }

        var (success, message, data) = await _authService.VerifyEmailAsync(token);

        if (!success)
        {
            return BadRequest(new { message });
        }

        // Return HTML page with success message and hidden token (show on click)
        return Content($@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>Email Verified - StudyMate</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            padding: 20px;
        }}
        .container {{
            background: white;
            padding: 50px 40px;
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            text-align: center;
            max-width: 500px;
            width: 100%;
            animation: slideIn 0.5s ease-out;
        }}
        @keyframes slideIn {{
            from {{
                opacity: 0;
                transform: translateY(-30px);
            }}
            to {{
                opacity: 1;
                transform: translateY(0);
            }}
        }}
        .success-icon {{
            width: 80px;
            height: 80px;
            background: #4CAF50;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 30px;
            animation: scaleIn 0.5s ease-out 0.2s both;
        }}
        @keyframes scaleIn {{
            from {{
                transform: scale(0);
            }}
            to {{
                transform: scale(1);
            }}
        }}
        .success-icon::after {{
            content: '?';
            color: white;
            font-size: 48px;
            font-weight: bold;
        }}
        h1 {{
            color: #333;
            margin-bottom: 15px;
            font-size: 28px;
        }}
        p {{
            color: #666;
            line-height: 1.8;
            margin-bottom: 20px;
            font-size: 16px;
        }}
        .success-message {{
            background: #e8f5e9;
            padding: 15px;
            border-radius: 10px;
            margin: 20px 0;
            border-left: 4px solid #4CAF50;
        }}
        .success-message p {{
            color: #2e7d32;
            margin: 0;
            font-weight: 500;
        }}
        .token-section {{
            margin: 30px 0;
        }}
        .show-token-btn {{
            background: #667eea;
            color: white;
            border: none;
            padding: 12px 30px;
            border-radius: 8px;
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
        }}
        .show-token-btn:hover {{
            background: #5568d3;
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
        }}
        .show-token-btn:active {{
            transform: translateY(0);
        }}
        .token-container {{
            display: none;
            background: #f5f5f5;
            padding: 20px;
            border-radius: 10px;
            margin-top: 15px;
            max-height: 150px;
            overflow-y: auto;
            border: 1px solid #ddd;
        }}
        .token-container.show {{
            display: block;
            animation: fadeIn 0.3s ease-out;
        }}
        @keyframes fadeIn {{
            from {{
                opacity: 0;
            }}
            to {{
                opacity: 1;
            }}
        }}
        .token-label {{
            font-weight: 600;
            color: #555;
            margin-bottom: 10px;
            font-size: 14px;
        }}
        .token-value {{
            word-break: break-all;
            font-size: 11px;
            color: #666;
            font-family: 'Courier New', monospace;
            line-height: 1.6;
            background: white;
            padding: 10px;
            border-radius: 5px;
        }}
        .copy-btn {{
            background: #4CAF50;
            color: white;
            border: none;
            padding: 8px 20px;
            border-radius: 5px;
            font-size: 12px;
            cursor: pointer;
            margin-top: 10px;
            transition: background 0.3s;
        }}
        .copy-btn:hover {{
            background: #45a049;
        }}
        .copy-btn.copied {{
            background: #2196F3;
        }}
        .instruction {{
            background: #fff3cd;
            padding: 15px;
            border-radius: 8px;
            margin-top: 20px;
            border-left: 4px solid #ffc107;
        }}
        .instruction p {{
            color: #856404;
            margin: 0;
            font-size: 14px;
        }}
        .close-text {{
            color: #999;
            font-size: 14px;
            margin-top: 30px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='success-icon'></div>
        <h1>Email Verified Successfully!</h1>
        
        <div class='success-message'>
            <p>{message}</p>
        </div>
        
        <p>Your student account has been activated and you can now log in to StudyMate.</p>
        
        <div class='instruction'>
            <p><strong>Next Step:</strong> Use the Login API with your email and password to get your access token.</p>
        </div>
        
        <div class='token-section'>
            <button class='show-token-btn' onclick='toggleToken()'>
                <span id='btnText'>?? Show Access Token</span>
            </button>
            <div id='tokenContainer' class='token-container'>
                <div class='token-label'>Access Token:</div>
                <div class='token-value' id='tokenValue'>{data.AccessToken}</div>
                <button class='copy-btn' onclick='copyToken()' id='copyBtn'>?? Copy Token</button>
            </div>
        </div>
        
        <p class='close-text'>You can close this window and return to the application.</p>
    </div>

    <script>
        function toggleToken() {{
            const container = document.getElementById('tokenContainer');
            const btnText = document.getElementById('btnText');
            if (container.classList.contains('show')) {{
                container.classList.remove('show');
                btnText.textContent = '?? Show Access Token';
            }} else {{
                container.classList.add('show');
                btnText.textContent = '?? Hide Access Token';
            }}
        }}

        function copyToken() {{
            const tokenValue = document.getElementById('tokenValue').textContent;
            const copyBtn = document.getElementById('copyBtn');
            
            navigator.clipboard.writeText(tokenValue).then(() => {{
                copyBtn.textContent = '? Copied!';
                copyBtn.classList.add('copied');
                setTimeout(() => {{
                    copyBtn.textContent = '?? Copy Token';
                    copyBtn.classList.remove('copied');
                }}, 2000);
            }}).catch(err => {{
                alert('Failed to copy token. Please copy manually.');
            }});
        }}
    </script>
</body>
</html>
        ", "text/html");
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, message, data) = await _authService.LoginAsync(loginDto);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new
        {
            message,
            data
        });
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, message, data) = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new
        {
            message,
            data
        });
    }

    /// <summary>
    /// Logout - revoke all refresh tokens for the user
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var (success, message) = await _authService.LogoutAsync(userId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { message });
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var user = await _authService.GetUserByIdAsync(userId);
        
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(new
        {
            message = "User retrieved successfully",
            data = user
        });
    }

    /// <summary>
    /// Admin: Upgrade a student to teacher
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("upgrade-to-teacher")]
    public async Task<IActionResult> UpgradeToTeacher([FromBody] UpgradeToTeacherDto upgradeDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, message) = await _authService.UpgradeToTeacherAsync(upgradeDto.StudentUserId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { message });
    }

    /// <summary>
    /// Request password reset - sends OTP code to email
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, message) = await _authService.ForgotPasswordAsync(forgotPasswordDto);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { message });
    }

    /// <summary>
    /// Reset password using OTP code
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, message) = await _authService.ResetPasswordAsync(resetPasswordDto);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { message });
    }
}
