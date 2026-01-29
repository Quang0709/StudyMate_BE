using System;

namespace Repository.Model;

public partial class Flashcard
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Front { get; set; } = null!; // Front of the flashcard

    public string Back { get; set; } = null!; // Back of the flashcard (definition/answer)

    public virtual Course Course { get; set; } = null!;
}
