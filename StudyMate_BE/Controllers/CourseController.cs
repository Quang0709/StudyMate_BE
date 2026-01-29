using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interface;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudyMate_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "teacher")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>
    /// Create a new course with sections, quizzes and flashcards (Teacher only)
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/course/create
    ///     {
    ///         "title": "Introduction to Programming",
    ///         "description": "Learn the basics of programming",
    ///         "price": 99.99,
    ///         "sections": [
    ///             {
    ///                 "title": "Section 1: Getting Started",
    ///                 "description": "Introduction to programming concepts",
    ///                 "orderIndex": 1,
    ///                 "materials": [
    ///                     {
    ///                         "title": "Course Introduction PDF",
    ///                         "description": "Overview of the course",
    ///                         "materialType": "pdf",
    ///                         "fileUrl": "https://example.com/files/intro.pdf",
    ///                         "fileName": "intro.pdf",
    ///                         "fileSize": 2048576,
    ///                         "orderIndex": 1
    ///                     },
    ///                     {
    ///                         "title": "Setup Video Tutorial",
    ///                         "materialType": "video",
    ///                         "fileUrl": "https://example.com/videos/setup.mp4",
    ///                         "orderIndex": 2
    ///                     }
    ///                 ]
    ///             }
    ///         ],
    ///         "quizzes": [
    ///             {
    ///                 "title": "Quiz 1: Basics",
    ///                 "questions": [
    ///                     {
    ///                         "questionText": "What is a variable?",
    ///                         "questionType": "multiple_choice",
    ///                         "options": [
    ///                             { "optionText": "A storage location", "isCorrect": true },
    ///                             { "optionText": "A function", "isCorrect": false },
    ///                             { "optionText": "A loop", "isCorrect": false },
    ///                             { "optionText": "A class", "isCorrect": false }
    ///                         ]
    ///                     }
    ///                 ]
    ///             }
    ///         ],
    ///         "flashcards": [
    ///             {
    ///                 "front": "What is OOP?",
    ///                 "back": "Object-Oriented Programming"
    ///             }
    ///         ]
    ///     }
    ///     
    /// **Limitations:**
    /// - Maximum 50 questions per quiz
    /// - Maximum 50 flashcards per course
    /// - Supported material types: pdf, video, document, link, image, other
    /// </remarks>
    [HttpPost("create")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto createCourseDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get user ID from JWT claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        if (roleClaim != "teacher")
        {
            return Forbid();
        }

        var (success, message, data) = await _courseService.CreateCourseAsync(userId, createCourseDto);

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
}
