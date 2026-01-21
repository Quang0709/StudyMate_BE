using System.ComponentModel.DataAnnotations;

namespace Service.DTOs;

public class RefreshTokenDto
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = null!;
}
