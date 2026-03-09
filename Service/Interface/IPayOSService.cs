using Service.DTOs;
using System.Threading.Tasks;

namespace Service.Interface;

public interface IPayOSService
{
    /// <summary>
    /// T?o payment link PayOS cho course purchase ho?c subscription
    /// </summary>
    Task<(bool success, string message, PaymentLinkDto? data)> CreatePaymentLinkAsync(
        int userId, string orderType, int referenceId, decimal amount, string description);

    /// <summary>
    /// X? lý webhook callback t? PayOS
    /// </summary>
    Task<(bool success, string message)> HandleWebhookAsync(PayOSWebhookDto webhook);

    /// <summary>
    /// Xác nh?n tr?ng thái payment v?i PayOS (dùng ?? verify sau khi returnUrl callback)
    /// </summary>
    Task<(bool success, string message, string? status)> VerifyPaymentAsync(long orderCode);
}
