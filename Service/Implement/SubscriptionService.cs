using Repository.Interface;
using Service.Interface;

public class SubscriptionService : ISubscriptionService
{
    private readonly IUserSubscriptionRepository _subscriptionRepository;

    public SubscriptionService(IUserSubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<bool> HasActiveSubscriptionAsync(int userId, string role)
    {
        var subscription = await _subscriptionRepository
            .GetActiveSubscriptionAsync(userId, role);

        return subscription != null;
    }
}