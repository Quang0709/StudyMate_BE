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
    // ======================================================
    // 1️⃣ TẠO LINK THANH TOÁN
    // ======================================================
    [Authorize]
    [HttpPost("create-payment/{courseId}")]
    public async Task<IActionResult> CreatePayment(int courseId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _coursePurchaseService
            .CreateCoursePaymentAsync(userId, courseId);

        return Ok(result);
    }

    // ======================================================
    // 2️⃣ MUA KHÓA HỌC (MANUAL COMPLETE - nếu cần)
    // ======================================================
    [Authorize]
    [HttpPost("complete/{courseId}")]
    public async Task<IActionResult> CompletePurchase(int courseId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _coursePurchaseService
            .PurchaseCourseAsync(userId, courseId);

        if (!result.success)
            return BadRequest(result.message);

        return Ok(result);
    }

    // ======================================================
    // 3️⃣ HỦY PAYMENT
    // ======================================================
    [Authorize]
    [HttpPost("cancel/{orderCode}")]
    public async Task<IActionResult> CancelPayment(long orderCode)
    {
        await _coursePurchaseService.CancelPaymentAsync(orderCode);
        return Ok("Payment cancelled");
    }

    // ======================================================
    // 4️⃣ CHECK ĐÃ MUA
    // ======================================================
    [Authorize]
    [HttpGet("check/{courseId}")]
    public async Task<IActionResult> Check(int courseId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _coursePurchaseService
            .CheckCoursePurchasedAsync(userId, courseId);

        return Ok(result);
    }

    // ======================================================
    // 5️⃣ LẤY DANH SÁCH ĐÃ MUA
    // ======================================================
    [Authorize]
    [HttpGet("my-courses")]
    public async Task<IActionResult> GetMyCourses()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _coursePurchaseService
            .GetMyPurchasedCoursesAsync(userId);

        return Ok(result);
    }

    // ======================================================
    // 6️⃣ SUMMARY
    // ======================================================
    [Authorize]
    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _coursePurchaseService
            .GetMyPurchasesSummaryAsync(userId);

        return Ok(result);
    }
}


