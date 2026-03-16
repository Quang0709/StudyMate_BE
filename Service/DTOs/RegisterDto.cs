using System.ComponentModel.DataAnnotations;

namespace Service.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(255, MinimumLength = 2)]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = null!;
}

public class UpgradeToTeacherDto
{
    [Required(ErrorMessage = "Student ID is required")]
    public int StudentUserId { get; set; }
}
