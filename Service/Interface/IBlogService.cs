using Service.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interface;

public interface IBlogService
{
    // Post
    Task<(bool success, string message, PostResponseDto? data)> CreatePostAsync(int authorId, CreatePostDto dto);
    Task<(bool success, string message, PostResponseDto? data)> GetPostByIdAsync(int postId, int? currentUserId);
    Task<(bool success, string message, int totalCount, List<PostResponseDto>? data)> GetAllPostsAsync(int? currentUserId, int page, int pageSize);
    Task<(bool success, string message, List<PostResponseDto>? data)> GetPostsByUserAsync(int userId, int? currentUserId);
    Task<(bool success, string message, PostResponseDto? data)> UpdatePostAsync(int postId, int currentUserId, UpdatePostDto dto);
    Task<(bool success, string message)> DeletePostAsync(int postId, int currentUserId);

    // Like
    Task<(bool success, string message, int likeCount)> ToggleLikeAsync(int postId, int userId);

    // Comment
    Task<(bool success, string message, CommentResponseDto? data)> AddCommentAsync(int postId, int authorId, CreateCommentDto dto);
    Task<(bool success, string message, List<CommentResponseDto>? data)> GetCommentsByPostAsync(int postId);
}
