using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interface;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudyMate_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;private readonly ICourse2Service _course2Service;


    public CourseController(ICourseService courseService, ICourse2Service course2Service)
    {
        _courseService = courseService;
        _course2Service = course2Service;
    }

    /// <summary>
    /// Get all approved and active courses (Public access - no authentication required)
    /// </summary>
    /// <remarks>
    /// This endpoint returns all courses that are:
    /// - Status: "approved"
    /// - Not deleted (IsDeleted = false)
    /// 
    /// Includes:
    /// - Course basic information
    /// - Teacher information (name and avatar)
    /// - Average rating and total ratings
    /// - Statistics (sections, materials, quizzes, questions, flashcards, enrollments)
    /// - Full course content (sections with materials, quizzes with questions, flashcards)
    /// 
    /// No authentication required - anyone can view public courses.
    /// </remarks>
    /// <response code="200">Returns list of courses successfully</response>
    /// <response code="400">If there's an error retrieving courses</response>
    [HttpGet("all")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetAllCourses()
    {
        var (success, message, data) = await _courseService.GetAllPublicCoursesAsync();

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
    /// Get all active courses created by the logged-in teacher (excludes soft-deleted courses)
    /// </summary>
    [HttpGet("my-courses")]
    [Authorize(Roles = "teacher")]
    public async Task<IActionResult> GetMyCourses()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var (success, message, data) = await _courseService.GetMyCoursesAsync(userId);

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
    /// Get detailed information of a specific course by ID (Teacher only - must be owner)
    /// </summary>
    /// <param name="id">Course ID</param>
    [HttpGet("{id}")]
    [Authorize(Roles = "teacher")]
    public async Task<IActionResult> GetCourseDetail(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var (success, message, data) = await _courseService.GetCourseDetailAsync(id, userId);

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
    [Authorize(Roles = "teacher")]
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

    /// <summary>
    /// Update an existing course with all its contents (Teacher only - must be owner)
    /// </summary>
    /// <param name="id">Course ID</param>
    /// <param name="updateCourseDto">Course update data</param>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/course/update/1
    ///     {
    ///         "title": "Updated Course Title",
    ///         "description": "Updated description",
    ///         "price": 149.99,
    ///         "categoryId": 2,
    ///         "sections": [
    ///             {
    ///                 "id": 1,
    ///                 "title": "Updated Section 1",
    ///                 "description": "Updated section description",
    ///                 "orderIndex": 1,
    ///                 "materials": [
    ///                     {
    ///                         "id": 1,
    ///                         "title": "Updated Material",
    ///                         "description": "Updated material description",
    ///                         "materialType": "pdf",
    ///                         "fileUrl": "https://example.com/updated.pdf",
    ///                         "fileName": "updated.pdf",
    ///                         "fileSize": 3048576,
    ///                         "orderIndex": 1
    ///                     },
    ///                     {
    ///                         "title": "New Material",
    ///                         "materialType": "video",
    ///                         "fileUrl": "https://example.com/new-video.mp4",
    ///                         "orderIndex": 2
    ///                     }
    ///                 ]
    ///             },
    ///             {
    ///                 "title": "New Section 2",
    ///                 "description": "Brand new section",
    ///                 "orderIndex": 2,
    ///                 "materials": []
    ///             }
    ///         ],
    ///         "quizzes": [
    ///             {
    ///                 "id": 1,
    ///                 "title": "Updated Quiz 1",
    ///                 "questions": [
    ///                     {
    ///                         "id": 1,
    ///                         "questionText": "Updated question?",
    ///                         "questionType": "multiple_choice",
    ///                         "options": [
    ///                             { "id": 1, "optionText": "Updated option 1", "isCorrect": true },
    ///                             { "id": 2, "optionText": "Updated option 2", "isCorrect": false },
    ///                             { "optionText": "New option 3", "isCorrect": false }
    ///                         ]
    ///                     }
    ///                 ]
    ///             }
    ///         ],
    ///         "flashcards": [
    ///             {
    ///                 "id": 1,
    ///                 "front": "Updated front",
    ///                 "back": "Updated back"
    ///             },
    ///             {
    ///                 "front": "New flashcard front",
    ///                 "back": "New flashcard back"
    ///             }
    ///         ]
    ///     }
    ///     
    /// **Important Notes:**
    /// - Include "id" field for existing items to update them
    /// - Omit "id" field for new items to create them
    /// - Items not included in the request will be deleted
    /// - Maximum 50 questions per quiz
    /// - Maximum 50 flashcards per course
    /// - Only the course owner can update
    /// </remarks>
    [HttpPut("update/{id}")]
    [Authorize(Roles = "teacher")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto updateCourseDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get user ID from JWT claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var (success, message, data) = await _courseService.UpdateCourseAsync(id, userId, updateCourseDto);

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
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchCourseDto dto)
    {
        var result = await _course2Service.SearchAsync(dto);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDeleteCourse(int id)
    {
        // Get user ID from JWT claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var (success, message) = await _course2Service.SoftDeleteAsync(id, userId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new
        {
            message
        });
    }
}
