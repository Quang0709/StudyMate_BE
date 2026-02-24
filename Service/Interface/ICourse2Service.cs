using Service.DTOs;

namespace Service.Interface;

public interface ICourse2Service
{
    Task<List<CourseSearchResultDto>> SearchAsync(SearchCourseDto dto);
    Task<(bool success, string message)> SoftDeleteAsync(int courseId, int userId);
    Task<(bool success, string message)> EnrollCourseAsync(int courseId, int userId, string role);
}
