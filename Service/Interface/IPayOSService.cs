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

    // ===================== ADMIN =====================

    Task<PaymentOrderListDto> GetPaymentOrdersAsync(PaymentFilterDto filter);

    Task<PaymentOrderDetailDto?> GetPaymentOrderByIdAsync(int id);

    Task<RevenueSummaryDto> GetRevenueSummaryAsync(int? year = null);

    /// <summary>
    /// Ch? ??ng g?i PayOS API ?? ??ng b? tr?ng thái các ??n pending trong DB
    /// </summary>
    Task<SyncPaymentResultDto> SyncPaymentStatusAsync();
}
