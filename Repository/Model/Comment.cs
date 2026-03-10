using System;
using System.Collections.Generic;

namespace Repository.Model;

public partial class Comment
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public int AuthorId { get; set; }

    public string? Content { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;
}
