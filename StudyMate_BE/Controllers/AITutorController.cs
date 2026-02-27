using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interface;
using System.Threading.Tasks;

namespace StudyMate_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AITutorController : ControllerBase
{
    private readonly IGeminiAIService _aiService;

    public AITutorController(IGeminiAIService aiService)
    {
        _aiService = aiService;
    }

    /// <summary>
    /// Chat with AI Tutor
    /// </summary>
    /// <remarks>
    /// Have a conversation with AI tutor. The AI will maintain context of the conversation.
    /// 
    /// Sample request:
    /// {
    ///     "message": "Can you explain what is recursion in programming?",
    ///     "conversationId": "optional-id-to-continue-conversation"
    /// }
    /// 
    /// Sample response:
    /// {
    ///     "message": "Chat response generated successfully",
    ///     "data": {
    ///         "response": "Recursion is a programming technique where a function calls itself...",
    ///         "conversationId": "550e8400-e29b-41d4-a716-446655440000",
    ///         "timestamp": "2024-01-15T10:30:00Z"
    ///     }
    /// }
    /// 
    /// To continue conversation, use the same conversationId in next request.
    /// </remarks>
    /// <response code="200">Returns AI response with conversation ID</response>
    /// <response code="400">Bad request or AI error</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("chat")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, message, data) = await _aiService.ChatAsync(request.Message, request.ConversationId);

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
