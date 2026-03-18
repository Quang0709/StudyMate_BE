using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interface;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudyMate_BE.Controllers;

[ApiController]
[Route("api/feedback")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "Invalid token" });

        var (success, message, data) = await _feedbackService.CreateFeedbackAsync(userId.Value, dto);
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message, data });
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyFeedback([FromBody] UpdateFeedbackDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "Invalid token" });

        var (success, message, data) = await _feedbackService.UpdateMyFeedbackAsync(userId.Value, dto);
        if (!success)
        {
            if (message.Contains("not found"))
                return NotFound(new { message });

            return BadRequest(new { message });
        }

        return Ok(new { message, data });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyFeedback()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "Invalid token" });

        var (success, message, data) = await _feedbackService.GetMyFeedbackAsync(userId.Value);
        if (!success)
            return NotFound(new { message });

        return Ok(new { message, data });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFeedbacks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var (success, message, totalCount, data) = await _feedbackService.GetAllFeedbacksAsync(page, pageSize);
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message, page, pageSize, count = totalCount, data });
    }

    [HttpGet("average-rating")]
    public async Task<IActionResult> GetAverageRating()
    {
        var (success, message, data) = await _feedbackService.GetAverageRatingAsync();
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message, data });
    }
}
