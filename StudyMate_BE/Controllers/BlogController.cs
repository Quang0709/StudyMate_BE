using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interface;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudyMate_BE.Controllers;

[ApiController]
[Route("api/blog")]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    // ==================== POST ENDPOINTS ====================

    /// <summary>
    /// T?o b�i vi?t blog m?i. Y�u c?u ??ng nh?p.
    /// </summary>
    [Authorize]
    [HttpPost("posts")]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "Invalid token" });

        var (success, message, data) = await _blogService.CreatePostAsync(userId.Value, dto);
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message, data });
    }

    /// <summary>
    /// L?y danh s�ch t?t c? b�i vi?t (ph�n trang). Kh�ng c?n ??ng nh?p,
    /// nh?ng n?u c� token th� s? tr? v? tr?ng th�i like c?a ng??i d�ng hi?n t?i.
    /// </summary>
    [HttpGet("posts")]
    public async Task<IActionResult> GetAllPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var currentUserId = GetCurrentUserId();
        var (success, message, totalCount, data) = await _blogService.GetAllPostsAsync(currentUserId, page, pageSize);
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message, page, pageSize, count = totalCount, data });
    }

    /// <summary>
    /// L?y chi ti?t m?t b�i vi?t theo ID.
    /// </summary>
    [HttpGet("posts/{postId:int}")]
    public async Task<IActionResult> GetPostById(int postId)
    {
        var currentUserId = GetCurrentUserId();
        var (success, message, data) = await _blogService.GetPostByIdAsync(postId, currentUserId);
        if (!success)
            return NotFound(new { message });

        return Ok(new { message, data });
    }

    /// <summary>
    /// L?y t?t c? b�i vi?t c?a m?t user c? th?.
    /// </summary>
    [HttpGet("users/{userId:int}/posts")]
    public async Task<IActionResult> GetPostsByUser(int userId)
    {
        var currentUserId = GetCurrentUserId();
        var (success, message, data) = await _blogService.GetPostsByUserAsync(userId, currentUserId);
        if (!success)
            return NotFound(new { message });

        return Ok(new { message, count = data?.Count ?? 0, data });
    }

    /// <summary>
    /// C?p nh?t b�i vi?t. Ch? t�c gi? m?i ???c ph�p ch?nh s?a.
    /// </summary>
    [Authorize]
    [HttpPut("posts/{postId:int}")]
    public async Task<IActionResult> UpdatePost(int postId, [FromBody] UpdatePostDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "Invalid token" });

        var (success, message, data) = await _blogService.UpdatePostAsync(postId, userId.Value, dto);
        if (!success)
        {
            if (message.Contains("not allowed"))
                return StatusCode(403, new { message });
            return NotFound(new { message });
        }

        return Ok(new { message, data });
    }

    /// <summary>
    /// Xo� b�i vi?t (soft delete). T�c gi? ho?c admin m?i ???c ph�p.
    /// </summary>
    [Authorize]
    [HttpDelete("posts/{postId:int}")]
    public async Task<IActionResult> DeletePost(int postId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "Invalid token" });

        var (success, message) = await _blogService.DeletePostAsync(postId, userId.Value);
        if (!success)
        {
            if (message.Contains("not allowed"))
                return StatusCode(403, new { message });
            return NotFound(new { message });
        }

        return Ok(new { message });
    }

    // ==================== LIKE ENDPOINTS ====================

    /// <summary>
    /// Like ho?c unlike b�i vi?t. N?u ?� like th� s? unlike v� ng??c l?i.
    /// </summary>
    [Authorize]
    [HttpPost("posts/{postId:int}/like")]
    public async Task<IActionResult> ToggleLike(int postId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "Invalid token" });

        var (success, message, likeCount) = await _blogService.ToggleLikeAsync(postId, userId.Value);
        if (!success)
            return NotFound(new { message });

        return Ok(new { message, likeCount });
    }

    // ==================== COMMENT ENDPOINTS ====================

    /// <summary>
    /// Th�m b�nh lu?n v�o b�i vi?t. Y�u c?u ??ng nh?p.
    /// </summary>
    [Authorize]
    [HttpPost("posts/{postId:int}/comments")]
    public async Task<IActionResult> AddComment(int postId, [FromBody] CreateCommentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "Invalid token" });

        var (success, message, data) = await _blogService.AddCommentAsync(postId, userId.Value, dto);
        if (!success)
            return NotFound(new { message });

        return Ok(new { message, data });
    }

    /// <summary>
    /// L?y danh s�ch b�nh lu?n c?a m?t b�i vi?t.
    /// </summary>
    [HttpGet("posts/{postId:int}/comments")]
    public async Task<IActionResult> GetComments(int postId)
    {
        var (success, message, data) = await _blogService.GetCommentsByPostAsync(postId);
        if (!success)
            return NotFound(new { message });

        return Ok(new { message, count = data?.Count ?? 0, data });
    }
}
