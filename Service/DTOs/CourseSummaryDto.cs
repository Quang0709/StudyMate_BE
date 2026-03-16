using System;

namespace Service.DTOs;

public class CourseSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? CategoryName { get; set; }
    public string? TeacherName { get; set; }
    public string? TeacherAvatar { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Statistics only
    public int TotalSections { get; set; }
    public int TotalMaterials { get; set; }
    public int TotalQuizzes { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalFlashcards { get; set; }
    public int TotalEnrollments { get; set; }
}
