using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Interface;
using Repository.Model;
using Service.DTOs;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Implement;

public class BlogService : IBlogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly StudyMateContext _context;

    public BlogService(IUnitOfWork unitOfWork, StudyMateContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    // ==================== POST ====================

    public async Task<(bool success, string message, PostResponseDto? data)> CreatePostAsync(int authorId, CreatePostDto dto)
    {
        try
        {
            var author = await _unitOfWork.Users.GetByIdAsync(authorId);
            if (author == null)
                return (false, "User not found", null);

            var post = new Post
            {
                AuthorId = authorId,
                Title = dto.Title,
                Content = dto.Content,
                ImageUrl = dto.ImageUrl,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Posts.AddAsync(post);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Post created successfully", MapPostToDto(post, author, 0, 0, false));
        }
        catch (Exception ex)
        {
            return (false, $"Error creating post: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message, PostResponseDto? data)> GetPostByIdAsync(int postId, int? currentUserId)
    {
        try
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.PostLikes)
                .Include(p => p.Comments.Where(c => !c.IsDeleted))
                .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);

            if (post == null)
                return (false, "Post not found", null);

            bool isLiked = currentUserId.HasValue && post.PostLikes.Any(l => l.UserId == currentUserId.Value);
            var dto = MapPostToDto(post, post.Author, post.PostLikes.Count, post.Comments.Count(c => !c.IsDeleted), isLiked);

            return (true, "Post retrieved successfully", dto);
        }
        catch (Exception ex)
        {
            return (false, $"Error retrieving post: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message, int totalCount, List<PostResponseDto>? data)> GetAllPostsAsync(int? currentUserId, int page, int pageSize)
    {
        try
        {
            var totalCount = await _context.Posts.CountAsync(p => !p.IsDeleted);

            var posts = await _context.Posts
                .Where(p => !p.IsDeleted)
                .Include(p => p.Author)
                .Include(p => p.PostLikes)
                .Include(p => p.Comments.Where(c => !c.IsDeleted))
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = posts.Select(p =>
            {
                bool isLiked = currentUserId.HasValue && p.PostLikes.Any(l => l.UserId == currentUserId.Value);
                return MapPostToDto(p, p.Author, p.PostLikes.Count, p.Comments.Count(c => !c.IsDeleted), isLiked);
            }).ToList();

            return (true, "Posts retrieved successfully", totalCount, result);
        }
        catch (Exception ex)
        {
            return (false, $"Error retrieving posts: {ex.Message}", 0, null);
        }
    }

    public async Task<(bool success, string message, List<PostResponseDto>? data)> GetPostsByUserAsync(int userId, int? currentUserId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return (false, "User not found", null);

            var posts = await _context.Posts
                .Where(p => p.AuthorId == userId && !p.IsDeleted)
                .Include(p => p.Author)
                .Include(p => p.PostLikes)
                .Include(p => p.Comments.Where(c => !c.IsDeleted))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var result = posts.Select(p =>
            {
                bool isLiked = currentUserId.HasValue && p.PostLikes.Any(l => l.UserId == currentUserId.Value);
                return MapPostToDto(p, p.Author, p.PostLikes.Count, p.Comments.Count(c => !c.IsDeleted), isLiked);
            }).ToList();

            return (true, "Posts retrieved successfully", result);
        }
        catch (Exception ex)
        {
            return (false, $"Error retrieving posts: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message, PostResponseDto? data)> UpdatePostAsync(int postId, int currentUserId, UpdatePostDto dto)
    {
        try
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.PostLikes)
                .Include(p => p.Comments.Where(c => !c.IsDeleted))
                .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);

            if (post == null)
                return (false, "Post not found", null);

            if (post.AuthorId != currentUserId)
                return (false, "You are not allowed to edit this post", null);

            if (dto.Title != null) post.Title = dto.Title;
            if (dto.Content != null) post.Content = dto.Content;
            if (dto.ImageUrl != null) post.ImageUrl = dto.ImageUrl;
            post.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Posts.Update(post);
            await _unitOfWork.SaveChangesAsync();

            bool isLiked = post.PostLikes.Any(l => l.UserId == currentUserId);
            return (true, "Post updated successfully", MapPostToDto(post, post.Author, post.PostLikes.Count, post.Comments.Count, isLiked));
        }
        catch (Exception ex)
        {
            return (false, $"Error updating post: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message)> DeletePostAsync(int postId, int currentUserId)
    {
        try
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);

            if (post == null)
                return (false, "Post not found");

            var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            bool isAdmin = user?.Role == "admin";

            if (post.AuthorId != currentUserId && !isAdmin)
                return (false, "You are not allowed to delete this post");

            post.IsDeleted = true;
            post.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Posts.Update(post);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Post deleted successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error deleting post: {ex.Message}");
        }
    }

    // ==================== LIKE ====================

    public async Task<(bool success, string message, int likeCount)> ToggleLikeAsync(int postId, int userId)
    {
        try
        {
            var post = await _unitOfWork.Posts.FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);
            if (post == null)
                return (false, "Post not found", 0);

            var existingLike = await _unitOfWork.PostLikes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            string message;
            if (existingLike != null)
            {
                _unitOfWork.PostLikes.Delete(existingLike);
                message = "Post unliked";
            }
            else
            {
                await _unitOfWork.PostLikes.AddAsync(new PostLike
                {
                    PostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
                message = "Post liked";
            }

            await _unitOfWork.SaveChangesAsync();

            var likeCount = (await _unitOfWork.PostLikes.FindAsync(l => l.PostId == postId)).Count();
            return (true, message, likeCount);
        }
        catch (Exception ex)
        {
            return (false, $"Error toggling like: {ex.Message}", 0);
        }
    }

    // ==================== COMMENT ====================

    public async Task<(bool success, string message, CommentResponseDto? data)> AddCommentAsync(int postId, int authorId, CreateCommentDto dto)
    {
        try
        {
            var post = await _unitOfWork.Posts.FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);
            if (post == null)
                return (false, "Post not found", null);

            var author = await _unitOfWork.Users.GetByIdAsync(authorId);
            if (author == null)
                return (false, "User not found", null);

            var comment = new Comment
            {
                PostId = postId,
                AuthorId = authorId,
                Content = dto.Content,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Comment added successfully", MapCommentToDto(comment, author));
        }
        catch (Exception ex)
        {
            return (false, $"Error adding comment: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message, List<CommentResponseDto>? data)> GetCommentsByPostAsync(int postId)
    {
        try
        {
            var post = await _unitOfWork.Posts.FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);
            if (post == null)
                return (false, "Post not found", null);

            var comments = await _context.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .Include(c => c.Author)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var result = comments.Select(c => MapCommentToDto(c, c.Author)).ToList();
            return (true, "Comments retrieved successfully", result);
        }
        catch (Exception ex)
        {
            return (false, $"Error retrieving comments: {ex.Message}", null);
        }
    }

    // ==================== MAPPING ====================

    private static PostResponseDto MapPostToDto(Post post, User author, int likeCount, int commentCount, bool isLiked)
    {
        return new PostResponseDto
        {
            Id = post.Id,
            AuthorId = post.AuthorId,
            AuthorName = author.FullName,
            AuthorAvatar = author.AvatarUrl,
            Title = post.Title,
            Content = post.Content,
            ImageUrl = post.ImageUrl,
            LikeCount = likeCount,
            CommentCount = commentCount,
            IsLikedByCurrentUser = isLiked,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }

    private static CommentResponseDto MapCommentToDto(Comment comment, User author)
    {
        return new CommentResponseDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            AuthorId = comment.AuthorId,
            AuthorName = author.FullName,
            AuthorAvatar = author.AvatarUrl,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }
}
