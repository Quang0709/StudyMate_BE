using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interface;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/course-student")]
[Authorize(Roles = "student")]
public class CourseStudentController : ControllerBase
{
    private readonly ICourse2Service _courseService;
    private readonly ISubscriptionService _subscriptionService;

    public CourseStudentController(
        ICourse2Service courseService,
        ISubscriptionService subscriptionService)
    {
        _courseService = courseService;
        _subscriptionService = subscriptionService;
    }

    // =========================
    // 1?? Xem t?t c? course
    // =========================
    [HttpGet("all")]
    public async Task<IActionResult> GetAllCourses()
    {
        var dto = new SearchCourseDto(); // kh�ng filter = l?y t?t c?
        var result = await _courseService.SearchAsync(dto);

        return Ok(result);
    }

    // =========================
    // 2?? Enroll course (Premium only)
    // =========================
    [HttpPost("{courseId}/enroll")]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)?.Trim().ToLower();

        if (string.IsNullOrEmpty(role))
            return Unauthorized("Invalid user role");

        var hasPremium = await _subscriptionService
            .HasActiveSubscriptionAsync(userId, role);

        if (!hasPremium)
            return BadRequest("Your premium subscription has expired");

        var result = await _courseService
            .EnrollCourseAsync(courseId, userId, role);

        if (!result.success)
            return BadRequest(result.message);

        return Ok("Enroll successfully");
    }

    [HttpGet("{courseId}/enrollment-status")]
    public async Task<IActionResult> GetEnrollmentStatus(int courseId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)?.Trim().ToLower();

        if (string.IsNullOrEmpty(role))
            return Unauthorized("Invalid user role");

        var result = await _courseService.GetEnrollmentStatusAsync(courseId, userId, role);

        if (!result.success)
            return BadRequest(new { message = result.message });

        return Ok(new
        {
            isEnrolled = result.isEnrolled,
            message = result.message
        });
    }
}