using System;
using System.Collections.Generic;

namespace Service.DTOs;

public class CreatePostDto
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? ImageUrl { get; set; }
}

public class UpdatePostDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
}

public class PostResponseDto
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = null!;
    public string? AuthorAvatar { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCommentDto
{
    public string Content { get; set; } = null!;
}

public class CommentResponseDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = null!;
    public string? AuthorAvatar { get; set; }
    public string? Content { get; set; }
    public DateTime? CreatedAt { get; set; }
}
