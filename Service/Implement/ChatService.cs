using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Model;

public class ChatService : IChatService
{
    private readonly StudyMateContext _context;

    public ChatService(StudyMateContext context)
    {
        _context = context;
    }

    public async Task<MessageResponse> SendMessageAsync(int senderId, SendMessageRequest request)
    {
        var message = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            ReceiverId = request.ReceiverId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);

        await _context.SaveChangesAsync();

        return new MessageResponse
        {
            Id = message.Id,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            IsRead = message.IsRead
        };
    }

    public async Task<List<MessageResponse>> GetConversationAsync(int user1, int user2)
    {
        return await _context.Messages
            .Where(m =>
                (m.SenderId == user1 && m.ReceiverId == user2) ||
                (m.SenderId == user2 && m.ReceiverId == user1))
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageResponse
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                IsRead = m.IsRead
            })
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(Guid messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);

        if (message == null)
            return;

        message.IsRead = true;

        await _context.SaveChangesAsync();
    }
}