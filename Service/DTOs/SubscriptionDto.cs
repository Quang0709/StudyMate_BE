using System;
using System.ComponentModel.DataAnnotations;

namespace Service.DTOs;

public class SubscribePlanDto
{
    [Required(ErrorMessage = "Plan ID is required")]
    public int PlanId { get; set; }
}

public class UserSubscriptionDto
{
    public int Id { get; set; }
    public int? StudentId { get; set; }
    public int? TeacherId { get; set; }
    public int PlanId { get; set; }
    public string? PlanName { get; set; }
    public int? DurationDays { get; set; }
    public decimal? Price { get; set; }
    public string? TargetRole { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int DaysRemaining { get; set; }
}
