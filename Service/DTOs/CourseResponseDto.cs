using System;
using System.Collections.Generic;

namespace Service.DTOs;

public class CourseResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = null!;
    public string? RejectReason { get; set; }
    public bool IsDeleted { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? TeacherName { get; set; }
    public string? TeacherAvatar { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Statistics
    public int TotalSections { get; set; }
    public int TotalMaterials { get; set; }
    public int TotalQuizzes { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalFlashcards { get; set; }
    public int TotalEnrollments { get; set; }
    
    // Details
    public List<CourseSectionDto> Sections { get; set; } = new();
    public List<QuizDto> Quizzes { get; set; } = new();
    public List<FlashcardDto> Flashcards { get; set; } = new();
}

public class CourseSectionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<SectionMaterialDto> Materials { get; set; } = new();
}

public class SectionMaterialDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string MaterialType { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public int OrderIndex { get; set; }
}

public class QuizDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public List<QuizQuestionDto> Questions { get; set; } = new();
}

public class QuizQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = null!;
    public string QuestionType { get; set; } = null!;
    public List<QuizOptionDto> Options { get; set; } = new();
}

public class QuizOptionDto
{
    public int Id { get; set; }
    public string OptionText { get; set; } = null!;
    public bool IsCorrect { get; set; }
}

public class FlashcardDto
{
    public int Id { get; set; }
    public string Front { get; set; } = null!;
    public string Back { get; set; } = null!;
}
