using System;

namespace Repository.Model;

public class OtpCode
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
    public DateTime ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsUsed { get; set; }
}
