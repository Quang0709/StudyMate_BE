using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interface;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudyMate_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PremiumPlanController : ControllerBase
{
    private readonly IPremiumPlanService _premiumPlanService;

    public PremiumPlanController(IPremiumPlanService premiumPlanService)
    {
        _premiumPlanService = premiumPlanService;
    }

    /// <summary>
    /// Get premium plans for the current logged-in user based on their role
    /// </summary>
    /// <remarks>
    /// Returns premium plans filtered by user's role:
    /// - If user is a Student ? returns plans with targetRole = "student"
    /// - If user is a Teacher ? returns plans with targetRole = "teacher"
    /// 
    /// Sample response for Student:
    /// {
    ///     "message": "Premium plans for student retrieved successfully",
    ///     "count": 3,
    ///     "data": [
    ///         {
    ///             "id": 1,
    ///             "name": "Student Basic",
    ///             "durationDays": 30,
    ///             "price": 99000,
    ///             "targetRole": "student"
    ///         },
    ///         {
    ///             "id": 2,
    ///             "name": "Student Pro",
    ///             "durationDays": 90,
    ///             "price": 249000,
    ///             "targetRole": "student"
    ///         }
    ///     ]
    /// }
    /// </remarks>
    /// <response code="200">Returns premium plans for user's role</response>
    /// <response code="401">Unauthorized - Invalid or missing token</response>
    /// <response code="400">Bad request - Error retrieving plans</response>
    [HttpGet("my-plans")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetMyPlans()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(roleClaim))
        {
            return Unauthorized(new { message = "Invalid token - role not found" });
        }

        var (success, message, data) = await _premiumPlanService.GetPlansByRoleAsync(roleClaim);

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
    /// Get premium plans by specific role (Public access)
    /// </summary>
    /// <param name="role">Role to filter plans: "student" or "teacher"</param>
    /// <remarks>
    /// Allows anyone to view premium plans for a specific role.
    /// Useful for displaying pricing pages on landing page.
    /// 
    /// Example: GET /api/premiumplan/by-role/student
    /// </remarks>
    /// <response code="200">Returns premium plans for specified role</response>
    /// <response code="400">Invalid role specified</response>
    [HttpGet("by-role/{role}")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetPlansByRole(string role)
    {
        var (success, message, data) = await _premiumPlanService.GetPlansByRoleAsync(role.ToLower());

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
    /// Get all premium plans (Admin only)
    /// </summary>
    /// <response code="200">Returns all premium plans</response>
    /// <response code="400">Error retrieving plans</response>
    [HttpGet("all")]
    [AllowAnonymous]  // Change to [Authorize(Roles = "admin")] if you have admin role
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetAllPlans()
    {
        var (success, message, data) = await _premiumPlanService.GetAllPlansAsync();

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
    /// Get premium plan by ID
    /// </summary>
    /// <param name="id">Plan ID</param>
    /// <response code="200">Returns premium plan details</response>
    /// <response code="404">Plan not found</response>
    /// <response code="400">Error retrieving plan</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetPlanById(int id)
    {
        var (success, message, data) = await _premiumPlanService.GetPlanByIdAsync(id);

        if (!success)
        {
            if (message.Contains("not found"))
            {
                return NotFound(new { message });
            }
            return BadRequest(new { message });
        }

        return Ok(new
        {
            message,
            data
        });
    }

    /// <summary>
    /// Subscribe to a premium plan
    /// </summary>
    /// <remarks>
    /// Allows user to subscribe to a premium plan.
    /// 
    /// Rules:
    /// - Plan must match user's role (student plan for students, teacher plan for teachers)
    /// - User can only have ONE active subscription at a time
    /// - No payment required (free subscription for now)
    /// - Subscription will auto-expire after duration ends
    /// 
    /// Sample request:
    /// {
    ///     "planId": 1
    /// }
    /// </remarks>
    [HttpPost("subscribe")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SubscribeToPlan([FromBody] SubscribePlanDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        if (string.IsNullOrEmpty(roleClaim))
        {
            return Unauthorized(new { message = "Invalid token - role not found" });
        }

        var (success, message, data) = await _premiumPlanService.SubscribeToPlanAsync(userId, roleClaim, dto.PlanId);

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
    /// Get current active subscription
    /// </summary>
    /// <remarks>
    /// Retrieve the current active subscription for the logged-in user.
    /// 
    /// Returns subscription details such as:
    /// - Subscription ID
    /// - Associated premium plan
    /// - Start and end date
    /// - Status (active, expired, etc.)
    /// 
    /// </remarks>
    /// <response code="200">Returns active subscription details</response>
    /// <response code="401">Unauthorized - Invalid or missing token</response>
    /// <response code="404">Not found - Subscription does not exist or has expired</response>
    /// <response code="400">Bad request - Error retrieving subscription</response>
    [HttpGet("my-subscription")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetMySubscription()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        if (string.IsNullOrEmpty(roleClaim))
        {
            return Unauthorized(new { message = "Invalid token - role not found" });
        }

        var (success, message, data) = await _premiumPlanService.GetActiveSubscriptionAsync(userId, roleClaim);

        if (!success)
        {
            if (message.Contains("not found") || message.Contains("expired"))
            {
                return NotFound(new { message });
            }
            return BadRequest(new { message });
        }

        return Ok(new
        {
            message,
            data
        });
    }

    /// <summary>
    /// Cancel current active subscription
    /// </summary>
    /// <remarks>
    /// Cancels the currently active subscription for the logged-in user.
    /// 
    /// After cancellation:
    /// - Subscription will no longer be active
    /// - User will not be billed
    /// - Access to premium features will continue until the end of the current billing cycle
    /// 
    /// </remarks>
    /// <response code="200">Cancellation successful</response>
    /// <response code="401">Unauthorized - Invalid or missing token</response>
    /// <response code="400">Bad request - Error cancelling subscription</response>
    [HttpPost("cancel-subscription")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CancelSubscription()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        if (string.IsNullOrEmpty(roleClaim))
        {
            return Unauthorized(new { message = "Invalid token - role not found" });
        }

        var (success, message) = await _premiumPlanService.CancelSubscriptionAsync(userId, roleClaim);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { message });
    }

    /// <summary>
    /// T?o payment link PayOS ?? subscribe premium plan
    /// </summary>
    /// <remarks>
    /// T?o link thanh toán qua PayOS. Sau khi thanh toán thành công,
    /// PayOS s? g?i webhook v? /api/payment/webhook ?? kích ho?t subscription.
    /// 
    /// Sample request:
    /// {
    ///     "planId": 1
    /// }
    /// </remarks>
    [HttpPost("create-payment")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateSubscriptionPayment([FromBody] CreateSubscriptionPaymentDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(new { message = "Invalid token" });

        if (string.IsNullOrEmpty(roleClaim))
            return Unauthorized(new { message = "Invalid token - role not found" });

        var (success, message, data) = await _premiumPlanService.CreateSubscriptionPaymentAsync(userId, roleClaim, dto.PlanId);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message, data });
    }
}
