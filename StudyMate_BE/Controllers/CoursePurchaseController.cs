using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interface;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudyMate_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursePurchaseController : ControllerBase
{
    private readonly ICoursePurchaseService _coursePurchaseService;

    public CoursePurchaseController(ICoursePurchaseService coursePurchaseService)
    {
        _coursePurchaseService = coursePurchaseService;
    }

    /// <summary>
    /// Purchase a course (Students only)
    /// </summary>
    /// <remarks>
    /// Allows student to purchase a course and get lifetime access.
    /// 
    /// Requirements:
    /// - User must be a Student
    /// - Course must be approved and not deleted
    /// - Course must have a price (> 0)
    /// - Student must not have purchased this course before
    /// 
    /// No real payment required (auto-completed for now).
    /// After purchase, student will be automatically enrolled.
    /// 
    /// Sample request:
    /// {
    ///     "courseId": 1
    /// }
    /// </remarks>
    [HttpPost("purchase")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> PurchaseCourse([FromBody] PurchaseCourseDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var (success, message, data) = await _coursePurchaseService.PurchaseCourseAsync(userId, dto.CourseId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new
        {
            message,
            data
        });
    }

    /// <summary>
    /// Get all purchased courses of current student
    /// </summary>
    [HttpGet("my-purchases")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> GetMyPurchasedCourses()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var (success, message, data) = await _coursePurchaseService.GetMyPurchasedCoursesAsync(userId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new
        {
            message,
            count = data?.Count ?? 0,
            data
        });
    }

    /// <summary>
    /// Check if student has purchased a specific course
    /// </summary>
    /// <param name="courseId">Course ID to check</param>
    [HttpGet("check-purchased/{courseId}")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> CheckCoursePurchased(int courseId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var (success, message) = await _coursePurchaseService.CheckCoursePurchasedAsync(userId, courseId);

        return Ok(new
        {
            purchased = success,
            message
        });
    }

    /// <summary>
    /// Get purchase summary (total courses purchased, total amount spent)
    /// </summary>
    [HttpGet("summary")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> GetPurchaseSummary()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var (success, message, data) = await _coursePurchaseService.GetMyPurchasesSummaryAsync(userId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new
        {
            message,
            data
        });
    }

    /// <summary>
    /// T?o payment link PayOS ?? mua course (Students only)
    /// </summary>
    /// <remarks>
    /// T?o link thanh toán qua PayOS. Sau khi thanh toán thành công,
    /// PayOS s? g?i webhook v? /api/payment/webhook ?? hoàn t?t giao d?ch
    /// và t? ??ng enroll student vào course.
    /// 
    /// Sample request:
    /// {
    ///     "courseId": 1
    /// }
    /// </remarks>
    [HttpPost("create-payment")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> CreateCoursePayment([FromBody] CreateCoursePaymentDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(new { message = "Invalid token" });

        var (success, message, data) = await _coursePurchaseService.CreateCoursePaymentAsync(userId, dto.CourseId);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message, data });
    }
}
