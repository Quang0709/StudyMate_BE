using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Service.DTOs;

public class CreateCourseDto
{
    [Required(ErrorMessage = "Course title is required")]
    [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
    public string Title { get; set; } = null!;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
    public decimal? Price { get; set; }

    public int? CategoryId { get; set; }

    public List<CreateCourseSectionDto>? Sections { get; set; }

    public List<CreateQuizDto>? Quizzes { get; set; }

    public List<CreateFlashcardDto>? Flashcards { get; set; }
}

public class UpdateCourseDto
{
    [Required(ErrorMessage = "Course title is required")]
    [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
    public string Title { get; set; } = null!;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
    public decimal? Price { get; set; }

    public int? CategoryId { get; set; }

    public List<UpdateCourseSectionDto>? Sections { get; set; }

    public List<UpdateQuizDto>? Quizzes { get; set; }

    public List<UpdateFlashcardDto>? Flashcards { get; set; }
}

public class CreateCourseSectionDto
{
    [Required(ErrorMessage = "Section title is required")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int OrderIndex { get; set; }

    public List<CreateSectionMaterialDto>? Materials { get; set; }
}

public class UpdateCourseSectionDto
{
    public int? Id { get; set; } // Null for new sections

    [Required(ErrorMessage = "Section title is required")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int OrderIndex { get; set; }

    public List<UpdateSectionMaterialDto>? Materials { get; set; }
}

public class CreateSectionMaterialDto
{
    [Required(ErrorMessage = "Material title is required")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Material type is required")]
    [RegularExpression("^(pdf|video|document|link|image|other)$", ErrorMessage = "Invalid material type")]
    public string MaterialType { get; set; } = null!;

    [Required(ErrorMessage = "File URL is required")]
    public string FileUrl { get; set; } = null!;

    public string? FileName { get; set; }

    public long? FileSize { get; set; }

    public int OrderIndex { get; set; }
}

public class UpdateSectionMaterialDto
{
    public int? Id { get; set; } // Null for new materials

    [Required(ErrorMessage = "Material title is required")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Material type is required")]
    [RegularExpression("^(pdf|video|document|link|image|other)$", ErrorMessage = "Invalid material type")]
    public string MaterialType { get; set; } = null!;

    [Required(ErrorMessage = "File URL is required")]
    public string FileUrl { get; set; } = null!;

    public string? FileName { get; set; }

    public long? FileSize { get; set; }

    public int OrderIndex { get; set; }
}

public class CreateQuizDto
{
    [Required(ErrorMessage = "Quiz title is required")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Required]
    [MinLength(1, ErrorMessage = "Quiz must have at least one question")]
    [MaxLength(50, ErrorMessage = "Quiz cannot have more than 50 questions")]
    public List<CreateQuizQuestionDto> Questions { get; set; } = new();
}

public class UpdateQuizDto
{
    public int? Id { get; set; } // Null for new quizzes

    [Required(ErrorMessage = "Quiz title is required")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Required]
    [MinLength(1, ErrorMessage = "Quiz must have at least one question")]
    [MaxLength(50, ErrorMessage = "Quiz cannot have more than 50 questions")]
    public List<UpdateQuizQuestionDto> Questions { get; set; } = new();
}

public class CreateQuizQuestionDto
{
    [Required(ErrorMessage = "Question text is required")]
    public string QuestionText { get; set; } = null!;

    [Required(ErrorMessage = "Question type is required")]
    [RegularExpression("^(multiple_choice|true_false)$", ErrorMessage = "Question type must be 'multiple_choice' or 'true_false'")]
    public string QuestionType { get; set; } = null!;

    [Required]
    [MinLength(2, ErrorMessage = "Question must have at least 2 options")]
    [MaxLength(6, ErrorMessage = "Question cannot have more than 6 options")]
    public List<CreateQuizOptionDto> Options { get; set; } = new();
}

public class UpdateQuizQuestionDto
{
    public int? Id { get; set; } // Null for new questions

    [Required(ErrorMessage = "Question text is required")]
    public string QuestionText { get; set; } = null!;

    [Required(ErrorMessage = "Question type is required")]
    [RegularExpression("^(multiple_choice|true_false)$", ErrorMessage = "Question type must be 'multiple_choice' or 'true_false'")]
    public string QuestionType { get; set; } = null!;

    [Required]
    [MinLength(2, ErrorMessage = "Question must have at least 2 options")]
    [MaxLength(6, ErrorMessage = "Question cannot have more than 6 options")]
    public List<UpdateQuizOptionDto> Options { get; set; } = new();
}

public class CreateQuizOptionDto
{
    [Required(ErrorMessage = "Option text is required")]
    public string OptionText { get; set; } = null!;

    public bool IsCorrect { get; set; }
}

public class UpdateQuizOptionDto
{
    public int? Id { get; set; } // Null for new options

    [Required(ErrorMessage = "Option text is required")]
    public string OptionText { get; set; } = null!;

    public bool IsCorrect { get; set; }
}

public class CreateFlashcardDto
{
    [Required(ErrorMessage = "Front content is required")]
    public string Front { get; set; } = null!;

    [Required(ErrorMessage = "Back content is required")]
    public string Back { get; set; } = null!;
}

public class UpdateFlashcardDto
{
    public int? Id { get; set; } // Null for new flashcards

    [Required(ErrorMessage = "Front content is required")]
    public string Front { get; set; } = null!;

    [Required(ErrorMessage = "Back content is required")]
    public string Back { get; set; } = null!;
}
