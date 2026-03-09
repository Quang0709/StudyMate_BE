using Service.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interface;

public interface ICourseService
{
    Task<(bool Success, string Message, object? Data)> CreateCourseAsync(int userId, CreateCourseDto createCourseDto);
    Task<(bool Success, string Message, object? Data)> UpdateCourseAsync(int courseId, int userId, UpdateCourseDto updateCourseDto);
    Task<(bool Success, string Message, List<CourseResponseDto>? Data)> GetMyCoursesAsync(int userId);
    Task<(bool Success, string Message, CourseResponseDto? Data)> GetCourseDetailAsync(int courseId, int userId);
    Task<(bool Success, string Message, List<CourseSummaryDto>? Data)> GetAllPublicCoursesAsync();
}
