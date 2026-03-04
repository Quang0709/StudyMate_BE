using System;

namespace Repository.Model;

public partial class PaymentOrder
{
    public int Id { get; set; }

    /// <summary>
    /// PayOS order code - unique long, dùng ?? ??nh danh v?i PayOS
    /// </summary>
    public long OrderCode { get; set; }

    public int UserId { get; set; }

    /// <summary>
    /// "course_purchase" ho?c "subscription"
    /// </summary>
    public string OrderType { get; set; } = null!;

    /// <summary>
    /// CourseId khi OrderType = "course_purchase", PlanId khi OrderType = "subscription"
    /// </summary>
    public int ReferenceId { get; set; }

    public decimal Amount { get; set; }

    /// <summary>
    /// "pending", "paid", "cancelled", "expired"
    /// </summary>
    public string Status { get; set; } = "pending";

    public string? PayOSTransactionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }

    public virtual User User { get; set; } = null!;
}
