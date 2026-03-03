using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interface;

namespace API.Controllers;

[Route("api/payment-webhook")]
[ApiController]
public class PaymentWebhookController : ControllerBase
{
    private readonly ICoursePurchaseService _coursePurchaseService;

    public PaymentWebhookController(ICoursePurchaseService coursePurchaseService)
    {
        _coursePurchaseService = coursePurchaseService;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook([FromBody] PayOSWebhookRequest request)
    {
        await _coursePurchaseService.HandleWebhookAsync(request);
        return Ok();
    }
}