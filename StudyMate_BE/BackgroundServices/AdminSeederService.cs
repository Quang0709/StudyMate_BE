using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using Repository.Model;

namespace StudyMate_BE.BackgroundServices;

/// <summary>
/// Background service to seed default admin user on application startup
/// </summary>
public class AdminSeederService : IHostedService
{
    private readonly ILogger<AdminSeederService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public AdminSeederService(
        ILogger<AdminSeederService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin Seeder Service started");

        try
        {
            await SeedAdminUserAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding admin user");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin Seeder Service stopped");
        return Task.CompletedTask;
    }

    private async Task SeedAdminUserAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            // Get admin config from appsettings
            var adminEmail = _configuration["DefaultAdmin:Email"];
            var adminPassword = _configuration["DefaultAdmin:Password"];
            var adminFullName = _configuration["DefaultAdmin:FullName"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                _logger.LogWarning("Default admin configuration not found in appsettings");
                return;
            }

            // Check if admin already exists
            var existingAdmin = await unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            
            if (existingAdmin != null)
            {
                _logger.LogInformation("Admin user already exists: {Email}", adminEmail);
                return;
            }

            // Create admin user
            var hashedPassword = HashPassword(adminPassword);
            
            var adminUser = new User
            {
                Email = adminEmail,
                Password = hashedPassword,
                FullName = adminFullName ?? "System Administrator",
                Role = "admin",
                IsBanned = false,
                CreatedAt = DateTime.UtcNow
            };

            await unitOfWork.Users.AddAsync(adminUser);
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Default admin user created successfully: {Email}", adminEmail);
            _logger.LogInformation("Admin credentials - Email: {Email}, Password: {Password}", adminEmail, adminPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating default admin user");
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
