namespace Service.DTOs;

public class PremiumPlanDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? DurationDays { get; set; }
    public decimal? Price { get; set; }
    public string? TargetRole { get; set; }  // "student" or "teacher"
}
