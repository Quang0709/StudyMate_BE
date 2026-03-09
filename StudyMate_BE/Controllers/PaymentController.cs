using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interface;
using System.Threading.Tasks;

namespace StudyMate_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPayOSService _payOSService;

    public PaymentController(IPayOSService payOSService)
    {
        _payOSService = payOSService;
    }

    /// <summary>
    /// Webhook endpoint nh?n callback t? PayOS sau khi thanh toán
    /// </summary>
    /// <remarks>
    /// Endpoint này ???c PayOS g?i t? ??ng khi có giao d?ch.
    /// Không c?n authentication.
    /// 
    /// PayOS s? POST JSON v?i c?u trúc:
    /// {
    ///     "code": "00",
    ///     "desc": "success",
    ///     "success": true,
    ///     "data": { "orderCode": 123456, "amount": 100000, ... },
    ///     "signature": "..."
    /// }
    /// </remarks>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookDto webhook)
    {
        var (success, message) = await _payOSService.HandleWebhookAsync(webhook);

        // PayOS yêu c?u tr? v? 200 OK k? c? khi x? lý th?t b?i (?? tránh retry loop)
        return Ok(new { success, message });
    }

    /// <summary>
    /// Verify tr?ng thái m?t payment theo orderCode
    /// </summary>
    /// <param name="orderCode">Order code t? PayOS</param>
    [HttpGet("verify/{orderCode}")]
    [Authorize]
    public async Task<IActionResult> VerifyPayment(long orderCode)
    {
        var (success, message, status) = await _payOSService.VerifyPaymentAsync(orderCode);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message, status });
    }
}
