using Service.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interface;

public interface IPremiumPlanService
{
    Task<(bool success, string message, List<PremiumPlanDto>? data)> GetPlansByRoleAsync(string role);
    Task<(bool success, string message, List<PremiumPlanDto>? data)> GetAllPlansAsync();
    Task<(bool success, string message, PremiumPlanDto? data)> GetPlanByIdAsync(int id);
    
    // Subscription methods
    Task<(bool success, string message, UserSubscriptionDto? data)> SubscribeToPlanAsync(int userId, string userRole, int planId);
    Task<(bool success, string message, UserSubscriptionDto? data)> GetActiveSubscriptionAsync(int userId, string userRole);
    Task<(bool success, string message)> CancelSubscriptionAsync(int userId, string userRole);
}
