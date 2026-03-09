using System;

namespace Repository.Model;

public partial class QuizOption
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public string OptionText { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public virtual QuizQuestion Question { get; set; } = null!;
}
