using System;
using System.Collections.Concurrent;

namespace Service.Helpers;

public class PendingRegistration
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public static class RegistrationCache
{
    private static readonly ConcurrentDictionary<string, PendingRegistration> _cache = new();

    public static void Add(string email, PendingRegistration registration)
    {
        _cache[email] = registration;
    }

    public static PendingRegistration? Get(string email)
    {
        if (_cache.TryGetValue(email, out var registration))
        {
            // Check if registration is still valid (within 15 minutes)
            if (DateTime.UtcNow - registration.CreatedAt < TimeSpan.FromMinutes(15))
            {
                return registration;
            }
            // Remove expired registration
            _cache.TryRemove(email, out _);
        }
        return null;
    }

    public static void Remove(string email)
    {
        _cache.TryRemove(email, out _);
    }
}
