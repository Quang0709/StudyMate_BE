using System;
using System.Collections.Generic;

namespace Repository.Model;

public partial class Post
{
    public int Id { get; set; }

    public int AuthorId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
}
