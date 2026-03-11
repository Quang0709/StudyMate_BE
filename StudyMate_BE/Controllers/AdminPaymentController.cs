using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interface;
using System.Threading.Tasks;

namespace StudyMate_BE.Controllers;

[ApiController]
[Route("api/admin/payment")]
[Authorize(Roles = "admin")]
public class AdminPaymentController : ControllerBase
{
    private readonly IPayOSService _payOSService;

    public AdminPaymentController(IPayOSService payOSService)
    {
        _payOSService = payOSService;
    }

    /// <summary>
    /// L?y danh sách t?t c? giao d?ch (có filter, paging)
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetPaymentOrders([FromQuery] PaymentFilterDto filter)
    {
        var result = await _payOSService.GetPaymentOrdersAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// L?y chi ti?t 1 giao d?ch theo ID
    /// </summary>
    [HttpGet("orders/{id:int}")]
    public async Task<IActionResult> GetPaymentOrderById(int id)
    {
        var result = await _payOSService.GetPaymentOrderByIdAsync(id);
        if (result == null)
            return NotFound(new { message = "Payment order not found" });

        return Ok(result);
    }

    /// <summary>
    /// Th?ng kê doanh thu t?ng quan (có th? l?c theo n?m)
    /// </summary>
    [HttpGet("revenue/summary")]
    public async Task<IActionResult> GetRevenueSummary([FromQuery] int? year = null)
    {
        var result = await _payOSService.GetRevenueSummaryAsync(year);
        return Ok(result);
    }

    /// <summary>
    /// ??ng b? tr?ng thái các ??n pending v?i PayOS (dùng khi webhook không nh?n ???c)
    /// </summary>
    [HttpPost("sync")]
    public async Task<IActionResult> SyncPaymentStatus()
    {
        var result = await _payOSService.SyncPaymentStatusAsync();
        return Ok(result);
    }
}
