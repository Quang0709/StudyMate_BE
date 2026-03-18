using Service.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interface;

public interface IFeedbackService
{
    Task<(bool success, string message, FeedbackResponseDto? data)> CreateFeedbackAsync(int userId, CreateFeedbackDto dto);
    Task<(bool success, string message, FeedbackResponseDto? data)> UpdateMyFeedbackAsync(int userId, UpdateFeedbackDto dto);
    Task<(bool success, string message, FeedbackResponseDto? data)> GetMyFeedbackAsync(int userId);
    Task<(bool success, string message, int totalCount, List<FeedbackResponseDto>? data)> GetAllFeedbacksAsync(int page, int pageSize);
    Task<(bool success, string message, FeedbackAverageDto data)> GetAverageRatingAsync();
}
