using System;
using System.ComponentModel.DataAnnotations;

namespace Service.DTOs;

public class PurchaseCourseDto
{
    [Required(ErrorMessage = "Course ID is required")]
    public int CourseId { get; set; }
}

public class CoursePurchaseDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public string? CourseTitle { get; set; }
    public string? CourseDescription { get; set; }
    public string? TeacherName { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string PaymentStatus { get; set; } = null!;
    public string? TransactionId { get; set; }
}

public class MyPurchasedCoursesDto
{
    public int TotalPurchased { get; set; }
    public decimal TotalSpent { get; set; }
    public List<CoursePurchaseDto> Purchases { get; set; } = new();
}
