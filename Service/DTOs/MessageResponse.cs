public class MessageResponse
{
    public Guid Id { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }
}

public class ConversationSummary
{
    public int OtherUserId { get; set; }

    public string OtherUserName { get; set; } = null!;

    public string? OtherUserAvatar { get; set; }

    public string LastMessage { get; set; } = null!;

    public DateTime LastMessageAt { get; set; }

    public int UnreadCount { get; set; }
}

public class UserSearchResultDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string Role { get; set; } = null!;
}