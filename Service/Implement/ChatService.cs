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
        var receiverExists = await _context.Users.AnyAsync(u => u.Id == request.ReceiverId);
        if (!receiverExists)
            throw new KeyNotFoundException($"User with id {request.ReceiverId} not found");

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

    public async Task<List<MessageResponse>> GetConversationAsync(int userId, int otherUserId)
    {
        return await _context.Messages
            .Where(m =>
                (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                (m.SenderId == otherUserId && m.ReceiverId == userId))
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

    public async Task<List<ConversationSummary>> GetConversationListAsync(int userId)
    {
        // Lấy tin nhắn mới nhất của mỗi cuộc hội thoại
        var messages = await _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .ToListAsync();

        // Group theo người còn lại trong cuộc hội thoại
        var conversations = messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g =>
            {
                var otherUserId = g.Key;
                var lastMsg = g.OrderByDescending(m => m.CreatedAt).First();
                var otherUser = lastMsg.SenderId == userId ? lastMsg.Receiver : lastMsg.Sender;
                var unreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead);

                return new ConversationSummary
                {
                    OtherUserId = otherUserId,
                    OtherUserName = otherUser.FullName,
                    OtherUserAvatar = otherUser.AvatarUrl,
                    LastMessage = lastMsg.Content,
                    LastMessageAt = lastMsg.CreatedAt,
                    UnreadCount = unreadCount
                };
            })
            .OrderByDescending(c => c.LastMessageAt)
            .ToList();

        return conversations;
    }

    public async Task MarkAsReadAsync(int userId, int otherUserId)
    {
        var unreadMessages = await _context.Messages
            .Where(m => m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead)
            .ToListAsync();

        if (!unreadMessages.Any())
            return;

        foreach (var msg in unreadMessages)
            msg.IsRead = true;

        await _context.SaveChangesAsync();
    }

    public async Task<List<UserSearchResultDto>> SearchUsersForChatAsync(int currentUserId, string keyword)
    {
        var trimmed = keyword.Trim().ToLower();

        return await _context.Users
            .Where(u =>
                u.Id != currentUserId &&
                (u.IsBanned == null || u.IsBanned == false) &&
                (u.FullName.ToLower().Contains(trimmed) ||
                 u.Email.ToLower().Contains(trimmed)))
            .OrderBy(u => u.FullName)
            .Take(20)
            .Select(u => new UserSearchResultDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                AvatarUrl = u.AvatarUrl,
                Role = u.Role
            })
            .ToListAsync();
    }
}