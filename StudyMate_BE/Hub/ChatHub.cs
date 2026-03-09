using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task SendMessage(int senderId, SendMessageRequest request)
    {
        var message = await _chatService.SendMessageAsync(senderId, request);

        await Clients.User(request.ReceiverId.ToString())
            .SendAsync("ReceiveMessage", message);

        await Clients.Caller
            .SendAsync("MessageSent", message);
    }
}