using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;
using System.Threading.Tasks;

namespace StudyMate_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get all categories (Public access - no authentication required)
    /// </summary>
    /// <remarks>
    /// Returns a list of all categories with ID and Name only (without Description).
    /// This endpoint is used by FE to display category options when creating or editing courses.
    /// 
    /// Sample response:
    /// {
    ///     "message": "Categories retrieved successfully",
    ///     "count": 5,
    ///     "data": [
    ///         { "id": 1, "name": "Programming" },
    ///         { "id": 2, "name": "Design" },
    ///         { "id": 3, "name": "Business" }
    ///     ]
    /// }
    /// </remarks>
    /// <response code="200">Returns list of categories successfully</response>
    /// <response code="400">If there's an error retrieving categories</response>
    [HttpGet("all")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetAllCategories()
    {
        var (success, message, data) = await _categoryService.GetAllCategoriesAsync();

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
}
