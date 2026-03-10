public interface IChatService
{
    Task<MessageResponse> SendMessageAsync(int senderId, SendMessageRequest request);

    Task<List<MessageResponse>> GetConversationAsync(int userId, int otherUserId);

    Task<List<ConversationSummary>> GetConversationListAsync(int userId);

    Task MarkAsReadAsync(int userId, int otherUserId);

    Task<List<UserSearchResultDto>> SearchUsersForChatAsync(int currentUserId, string keyword);
}