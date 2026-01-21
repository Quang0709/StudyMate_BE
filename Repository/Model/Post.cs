using System;
using System.Collections.Generic;

namespace Repository.Model;

public partial class Post
{
    public int Id { get; set; }

    public int AuthorId { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
