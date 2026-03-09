using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudyMate_BE.BackgroundServices;

public class SubscriptionExpiryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionExpiryService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every 1 hour

    public SubscriptionExpiryService(
        IServiceProvider serviceProvider,
        ILogger<SubscriptionExpiryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscription Expiry Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndExpireSubscriptions();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking expired subscriptions");
            }

            // Wait for next check
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Subscription Expiry Service stopped");
    }

    private async Task CheckAndExpireSubscriptions()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Get all active subscriptions that have expired
        var expiredSubscriptions = await unitOfWork.UserSubscriptions.FindAsync(
            s => s.IsActive && s.EndDate < DateTime.UtcNow
        );

        var expiredList = expiredSubscriptions.ToList();

        if (expiredList.Any())
        {
            _logger.LogInformation($"Found {expiredList.Count} expired subscriptions");

            foreach (var subscription in expiredList)
            {
                subscription.IsActive = false;
                unitOfWork.UserSubscriptions.Update(subscription);
                
                _logger.LogInformation(
                    $"Subscription ID {subscription.Id} has been deactivated (expired on {subscription.EndDate})");
            }

            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"Successfully deactivated {expiredList.Count} expired subscriptions");
        }
        else
        {
            _logger.LogDebug("No expired subscriptions found");
        }
    }
}
