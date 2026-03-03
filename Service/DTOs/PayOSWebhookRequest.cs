public class PayOSWebhookRequest
{
    public long orderCode { get; set; }
    public string status { get; set; } = null!;
    public string paymentLinkId { get; set; } = null!;
    public string signature { get; set; } = null!;
    public long OrderCode { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? TransactionId { get; set; }

    public decimal Amount { get; set; }
}