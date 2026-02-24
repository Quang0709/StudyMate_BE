using Repository.Model;

public interface IUserSubscriptionRepository
{
    Task<UserSubscription?> GetActiveSubscriptionAsync(int userId, string role);
}