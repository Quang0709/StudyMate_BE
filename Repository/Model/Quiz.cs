using System;
using System.Collections.Generic;

namespace Repository.Model;

public partial class Quiz
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string? Title { get; set; }

    public virtual Course Course { get; set; } = null!;
}
