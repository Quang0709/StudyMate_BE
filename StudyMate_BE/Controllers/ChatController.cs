using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
    {
        _chatService = chatService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// G?i tin nh?n ??n m?t user khác.
    /// senderId ???c l?y t? ??ng t? JWT token, không c?n truy?n th? công.
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var senderIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (senderIdClaim == null || !int.TryParse(senderIdClaim, out var senderId))
            return Unauthorized(new { message = "Invalid token" });

        if (senderId == request.ReceiverId)
            return BadRequest(new { message = "Cannot send message to yourself" });

        try
        {
            var message = await _chatService.SendMessageAsync(senderId, request);

            // ??y real-time ??n receiver n?u ?ang online
            await _hubContext.Clients.User(request.ReceiverId.ToString())
                .SendAsync("ReceiveMessage", message);

            return Ok(new { message = "Message sent successfully", data = message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// L?y toàn b? l?ch s? tin nh?n v?i m?t user c? th?.
    /// T? ??ng ?ánh d?u t?t c? tin nh?n ch?a ??c t? otherUserId là ?ã ??c.
    /// </summary>
    [HttpGet("conversation/{otherUserId:int}")]
    public async Task<IActionResult> GetConversation(int otherUserId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid token" });

        var messages = await _chatService.GetConversationAsync(userId, otherUserId);

        // T? ??ng mark as read khi m? cu?c h?i tho?i
        await _chatService.MarkAsReadAsync(userId, otherUserId);

        return Ok(new { data = messages });
    }

    /// <summary>
    /// L?y danh sách t?t c? cu?c h?i tho?i c?a user hi?n t?i,
    /// s?p x?p theo tin nh?n m?i nh?t, kèm s? tin nh?n ch?a ??c.
    /// </summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversationList()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid token" });

        var conversations = await _chatService.GetConversationListAsync(userId);

        return Ok(new { data = conversations });
    }

    /// <summary>
    /// Tìm ki?m user ?? b?t ??u cu?c h?i tho?i m?i.
    /// Tìm theo tên ho?c email, lo?i tr? chính mình và user b? banned.
    /// </summary>
    [HttpGet("search-users")]
    public async Task<IActionResult> SearchUsers([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Trim().Length < 2)
            return BadRequest(new { message = "Keyword must be at least 2 characters" });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var currentUserId))
            return Unauthorized(new { message = "Invalid token" });

        var users = await _chatService.SearchUsersForChatAsync(currentUserId, keyword);
        return Ok(new { data = users });
    }
}