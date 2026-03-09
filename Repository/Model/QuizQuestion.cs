using System;
using System.Collections.Generic;

namespace Repository.Model;

public partial class QuizQuestion
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string QuestionType { get; set; } = null!; // "multiple_choice", "true_false", etc.

    public DateTime? CreatedAt { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<QuizOption> QuizOptions { get; set; } = new List<QuizOption>();
}
