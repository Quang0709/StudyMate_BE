using System;
using System.Collections.Generic;

namespace Repository.Model;

public partial class Course
{
    public int Id { get; set; }

    public int TeacherId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Status { get; set; }

    public string? RejectReason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CourseRating> CourseRatings { get; set; } = new List<CourseRating>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();

    public virtual Teacher Teacher { get; set; } = null!;
}
