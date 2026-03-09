using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet("conversation")]
    public async Task<IActionResult> GetConversation(int user1, int user2)
    {
        var messages = await _chatService.GetConversationAsync(user1, user2);

        return Ok(messages);
    }
}