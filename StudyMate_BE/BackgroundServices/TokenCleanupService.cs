using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repository.Interface;

namespace StudyMate_BE.BackgroundServices;

/// <summary>
/// Background service to clean up expired OTP codes and refresh tokens
/// </summary>
public class TokenCleanupService : BackgroundService
{
    private readonly ILogger<TokenCleanupService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public TokenCleanupService(
        ILogger<TokenCleanupService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Token Cleanup Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredTokensAsync();
                await CleanupExpiredOtpCodesAsync();
                
                // Run cleanup every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up tokens");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Token Cleanup Service stopped");
    }

    private async Task CleanupExpiredTokensAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            var expiredTokens = await unitOfWork.RefreshTokens.FindAsync(
                rt => rt.ExpiryDate < DateTime.UtcNow || rt.IsRevoked
            );

            foreach (var token in expiredTokens)
            {
                unitOfWork.RefreshTokens.Delete(token);
            }

            var deletedCount = await unitOfWork.SaveChangesAsync();
            
            if (deletedCount > 0)
            {
                _logger.LogInformation($"Deleted {deletedCount} expired refresh tokens");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired refresh tokens");
        }
    }

    private async Task CleanupExpiredOtpCodesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            var expiredOtps = await unitOfWork.OtpCodes.FindAsync(
                otp => otp.ExpiryDate < DateTime.UtcNow || otp.IsUsed
            );

            foreach (var otp in expiredOtps)
            {
                unitOfWork.OtpCodes.Delete(otp);
            }

            var deletedCount = await unitOfWork.SaveChangesAsync();
            
            if (deletedCount > 0)
            {
                _logger.LogInformation($"Deleted {deletedCount} expired OTP codes");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired OTP codes");
        }
    }
}
