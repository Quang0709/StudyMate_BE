using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Service.DTOs;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Implement;

public class PremiumPlanService : IPremiumPlanService
{
    private readonly IUnitOfWork _unitOfWork;

    public PremiumPlanService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool success, string message, List<PremiumPlanDto>? data)> GetPlansByRoleAsync(string role)
    {
        try
        {
            // Validate role
            if (string.IsNullOrEmpty(role) || (role != "student" && role != "teacher"))
            {
                return (false, "Invalid role. Must be 'student' or 'teacher'", null);
            }

            // Get plans filtered by target role
            var plans = await _unitOfWork.PremiumPlans.FindAsync(p => p.TargetRole == role);

            var planDtos = plans
                .Select(p => new PremiumPlanDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    DurationDays = p.DurationDays,
                    Price = p.Price,
                    TargetRole = p.TargetRole
                })
                .OrderBy(p => p.Price)  // Sort by price
                .ToList();

            return (true, $"Premium plans for {role} retrieved successfully", planDtos);
        }
        catch (Exception ex)
        {
            return (false, $"Error retrieving premium plans: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message, List<PremiumPlanDto>? data)> GetAllPlansAsync()
    {
        try
        {
            var plans = await _unitOfWork.PremiumPlans.GetAllAsync();

            var planDtos = plans
                .Select(p => new PremiumPlanDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    DurationDays = p.DurationDays,
                    Price = p.Price,
                    TargetRole = p.TargetRole
                })
                .OrderBy(p => p.TargetRole)
                .ThenBy(p => p.Price)
                .ToList();

            return (true, "All premium plans retrieved successfully", planDtos);
        }
        catch (Exception ex)
        {
            return (false, $"Error retrieving premium plans: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message, PremiumPlanDto? data)> GetPlanByIdAsync(int id)
    {
        try
        {
            var plan = await _unitOfWork.PremiumPlans.GetByIdAsync(id);

            if (plan == null)
            {
                return (false, "Premium plan not found", null);
            }

            var planDto = new PremiumPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                DurationDays = plan.DurationDays,
                Price = plan.Price,
                TargetRole = plan.TargetRole
            };

            return (true, "Premium plan retrieved successfully", planDto);
        }
        catch (Exception ex)
        {
            return (false, $"Error retrieving premium plan: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message, UserSubscriptionDto? data)> SubscribeToPlanAsync(int userId, string userRole, int planId)
    {
        try
        {
            // Validate role
            if (userRole != "student" && userRole != "teacher")
            {
                return (false, "Invalid user role", null);
            }

            // Get plan
            var plan = await _unitOfWork.PremiumPlans.GetByIdAsync(planId);
            if (plan == null)
            {
                return (false, "Premium plan not found", null);
            }

            // Validate plan matches user role
            if (plan.TargetRole != userRole)
            {
                return (false, $"This plan is for {plan.TargetRole}, but you are a {userRole}", null);
            }

            // Get user's role-specific ID
            int? studentId = null;
            int? teacherId = null;

            if (userRole == "student")
            {
                var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student == null)
                {
                    return (false, "Student profile not found", null);
                }
                studentId = student.Id;

                // Check if student already has an active subscription
                var existingSubscription = await _unitOfWork.UserSubscriptions.FirstOrDefaultAsync(
                    s => s.StudentId == studentId && s.IsActive);
                
                if (existingSubscription != null)
                {
                    return (false, "You already have an active subscription. Please cancel it before subscribing to a new plan.", null);
                }
            }
            else // teacher
            {
                var teacher = await _unitOfWork.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
                if (teacher == null)
                {
                    return (false, "Teacher profile not found", null);
                }
                teacherId = teacher.Id;

                // Check if teacher already has an active subscription
                var existingSubscription = await _unitOfWork.UserSubscriptions.FirstOrDefaultAsync(
                    s => s.TeacherId == teacherId && s.IsActive);
                
                if (existingSubscription != null)
                {
                    return (false, "You already have an active subscription. Please cancel it before subscribing to a new plan.", null);
                }
            }

            // Create new subscription
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(plan.DurationDays ?? 30);

            var subscription = new Repository.Model.UserSubscription
            {
                StudentId = studentId,
                TeacherId = teacherId,
                PlanId = planId,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true
            };

            await _unitOfWork.UserSubscriptions.AddAsync(subscription);
            await _unitOfWork.SaveChangesAsync();

            var subscriptionDto = new UserSubscriptionDto
            {
                Id = subscription.Id,
                StudentId = studentId,
                TeacherId = teacherId,
                PlanId = planId,
                PlanName = plan.Name,
                DurationDays = plan.DurationDays,
                Price = plan.Price,
                TargetRole = plan.TargetRole,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                IsActive = subscription.IsActive,
                DaysRemaining = (int)(subscription.EndDate - DateTime.UtcNow).TotalDays
            };

            return (true, "Successfully subscribed to premium plan", subscriptionDto);
        }
        catch (Exception ex)
        {
            return (false, $"Error subscribing to plan: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message, UserSubscriptionDto? data)> GetActiveSubscriptionAsync(int userId, string userRole)
    {
        try
        {
            if (userRole != "student" && userRole != "teacher")
            {
                return (false, "Invalid user role", null);
            }

            Repository.Model.UserSubscription? subscription = null;

            if (userRole == "student")
            {
                var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student == null)
                {
                    return (false, "Student profile not found", null);
                }

                subscription = await _unitOfWork.UserSubscriptions.FirstOrDefaultAsync(
                    s => s.StudentId == student.Id && s.IsActive);
            }
            else // teacher
            {
                var teacher = await _unitOfWork.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
                if (teacher == null)
                {
                    return (false, "Teacher profile not found", null);
                }

                subscription = await _unitOfWork.UserSubscriptions.FirstOrDefaultAsync(
                    s => s.TeacherId == teacher.Id && s.IsActive);
            }

            if (subscription == null)
            {
                return (false, "No active subscription found", null);
            }

            // Check if subscription has expired
            if (subscription.EndDate < DateTime.UtcNow)
            {
                subscription.IsActive = false;
                _unitOfWork.UserSubscriptions.Update(subscription);
                await _unitOfWork.SaveChangesAsync();
                return (false, "Your subscription has expired", null);
            }

            var plan = await _unitOfWork.PremiumPlans.GetByIdAsync(subscription.PlanId);

            var subscriptionDto = new UserSubscriptionDto
            {
                Id = subscription.Id,
                StudentId = subscription.StudentId,
                TeacherId = subscription.TeacherId,
                PlanId = subscription.PlanId,
                PlanName = plan?.Name,
                DurationDays = plan?.DurationDays,
                Price = plan?.Price,
                TargetRole = plan?.TargetRole,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                IsActive = subscription.IsActive,
                DaysRemaining = Math.Max(0, (int)(subscription.EndDate - DateTime.UtcNow).TotalDays)
            };

            return (true, "Active subscription retrieved successfully", subscriptionDto);
        }
        catch (Exception ex)
        {
            return (false, $"Error retrieving subscription: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message)> CancelSubscriptionAsync(int userId, string userRole)
    {
        try
        {
            if (userRole != "student" && userRole != "teacher")
            {
                return (false, "Invalid user role");
            }

            Repository.Model.UserSubscription? subscription = null;

            if (userRole == "student")
            {
                var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student == null)
                {
                    return (false, "Student profile not found");
                }

                subscription = await _unitOfWork.UserSubscriptions.FirstOrDefaultAsync(
                    s => s.StudentId == student.Id && s.IsActive);
            }
            else // teacher
            {
                var teacher = await _unitOfWork.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
                if (teacher == null)
                {
                    return (false, "Teacher profile not found");
                }

                subscription = await _unitOfWork.UserSubscriptions.FirstOrDefaultAsync(
                    s => s.TeacherId == teacher.Id && s.IsActive);
            }

            if (subscription == null)
            {
                return (false, "No active subscription found");
            }

            subscription.IsActive = false;
            _unitOfWork.UserSubscriptions.Update(subscription);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Subscription cancelled successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error cancelling subscription: {ex.Message}");
        }
    }
}
