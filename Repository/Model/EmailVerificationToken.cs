using System;

namespace Repository.Model;

public class EmailVerificationToken
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsVerified { get; set; }
}
