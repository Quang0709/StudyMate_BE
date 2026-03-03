using System;

namespace Repository.Model;

public partial class CoursePurchase
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public decimal PurchasePrice { get; set; }  // Giá t?i th?i ?i?m mua

    public DateTime PurchaseDate { get; set; }

    public string PaymentStatus { get; set; } = "completed";  // "pending", "completed", "failed"

    public string? TransactionId { get; set; }  // ID giao d?ch (?? sau này tích h?p payment)

    public virtual Student Student { get; set; } = null!;

    public virtual Course Course { get; set; } = null!;
    public long OrderCode { get; set; }
    public string? PaymentLinkId { get; set; }


}
