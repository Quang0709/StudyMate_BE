public interface IChatService
{
    Task<MessageResponse> SendMessageAsync(int senderId, SendMessageRequest request);

    Task<List<MessageResponse>> GetConversationAsync(int user1, int user2);

    Task MarkAsReadAsync(Guid messageId);
}