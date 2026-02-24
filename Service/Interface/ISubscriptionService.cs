public interface ISubscriptionService
{
    Task<bool> HasActiveSubscriptionAsync(int userId, string role);
}