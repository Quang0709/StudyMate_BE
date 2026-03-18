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

public class FeedbackService : IFeedbackService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly StudyMateContext _context;

    public FeedbackService(IUnitOfWork unitOfWork, StudyMateContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<(bool success, string message, FeedbackResponseDto? data)> CreateFeedbackAsync(int userId, CreateFeedbackDto dto)
    {
        var validationError = ValidateFeedbackInput(dto.Rating, dto.Comment);
        if (!string.IsNullOrEmpty(validationError))
            return (false, validationError, null);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return (false, "User not found", null);

        var existingFeedback = await _unitOfWork.Feedbacks.FirstOrDefaultAsync(f => f.UserId == userId);
        if (existingFeedback != null)
            return (false, "You already created feedback. Please use update API.", null);

        var feedback = new Feedback
        {
            UserId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Feedbacks.AddAsync(feedback);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Feedback created successfully", MapToDto(feedback, user));
    }

    public async Task<(bool success, string message, FeedbackResponseDto? data)> UpdateMyFeedbackAsync(int userId, UpdateFeedbackDto dto)
    {
        var validationError = ValidateFeedbackInput(dto.Rating, dto.Comment);
        if (!string.IsNullOrEmpty(validationError))
            return (false, validationError, null);

        var feedback = await _unitOfWork.Feedbacks.FirstOrDefaultAsync(f => f.UserId == userId);
        if (feedback == null)
            return (false, "Feedback not found", null);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return (false, "User not found", null);

        feedback.Rating = dto.Rating;
        feedback.Comment = dto.Comment;
        feedback.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Feedbacks.Update(feedback);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Feedback updated successfully", MapToDto(feedback, user));
    }

    public async Task<(bool success, string message, FeedbackResponseDto? data)> GetMyFeedbackAsync(int userId)
    {
        var feedback = await _context.Feedbacks
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.UserId == userId);

        if (feedback == null)
            return (false, "Feedback not found", null);

        return (true, "Feedback retrieved successfully", MapToDto(feedback, feedback.User));
    }

    public async Task<(bool success, string message, int totalCount, List<FeedbackResponseDto>? data)> GetAllFeedbacksAsync(int page, int pageSize)
    {
        var totalCount = await _context.Feedbacks.CountAsync();

        var feedbacks = await _context.Feedbacks
            .Include(f => f.User)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = feedbacks.Select(f => MapToDto(f, f.User)).ToList();
        return (true, "Feedbacks retrieved successfully", totalCount, result);
    }

    public async Task<(bool success, string message, FeedbackAverageDto data)> GetAverageRatingAsync()
    {
        var average = await _context.Feedbacks
            .Select(f => (double?)f.Rating)
            .AverageAsync() ?? 0;

        return (true, "Average rating retrieved successfully", new FeedbackAverageDto
        {
            AverageRating = Math.Round(average, 2)
        });
    }

    private static string? ValidateFeedbackInput(int rating, string? comment)
    {
        if (rating < 1 || rating > 5)
            return "Rating must be an integer between 1 and 5";

        if (comment != null && comment.Length > 1000)
            return "Comment length must be less than or equal to 1000 characters";

        return null;
    }

    private static FeedbackResponseDto MapToDto(Feedback feedback, User user)
    {
        return new FeedbackResponseDto
        {
            Id = feedback.Id,
            UserId = feedback.UserId,
            UserName = user.FullName,
            Rating = feedback.Rating,
            Comment = feedback.Comment,
            CreatedAt = feedback.CreatedAt,
            UpdatedAt = feedback.UpdatedAt
        };
    }
}
