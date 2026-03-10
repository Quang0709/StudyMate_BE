using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task SendMessage(SendMessageRequest request)
    {
        var senderIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (senderIdClaim == null || !int.TryParse(senderIdClaim, out var senderId))
        {
            throw new HubException("Unauthorized");
        }

        var message = await _chatService.SendMessageAsync(senderId, request);

        await Clients.User(request.ReceiverId.ToString())
            .SendAsync("ReceiveMessage", message);

        await Clients.Caller
            .SendAsync("MessageSent", message);
    }
}