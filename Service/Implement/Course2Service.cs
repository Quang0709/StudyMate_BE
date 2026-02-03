using Repository.Interface;
using Service.DTOs;
using Service.Interface;

namespace Service.Implement;

public class Course2Service : ICourse2Service
{
    private readonly ICourseRepository _courseRepository;

    public Course2Service(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<List<CourseSearchResultDto>> SearchAsync(SearchCourseDto dto)
    {
        var courses = await _courseRepository.SearchAsync(
            dto.Title,
            dto.CategoryId,
            dto.TeacherName,
            dto.MinPrice,
            dto.MaxPrice
        );

        return courses.Select(c => new CourseSearchResultDto
        {
            Id = c.Id,
            Title = c.Title,
            Price = c.Price,
            CategoryName = c.Category?.Name,
            TeacherFullName = c.Teacher.User.FullName
        }).ToList();
    }
    public async Task<(bool success, string message)> SoftDeleteAsync(int courseId, int userId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);

        if (course == null || course.IsDeleted)
            return (false, "Course not found");

        bool isAdmin = course.Teacher.User.Role == "ADMIN";
        bool isOwner = course.Teacher.UserId == userId;

        if (!isAdmin && !isOwner)
            return (false, "You do not have permission to delete this course");

        course.IsDeleted = true;
        course.DeletedAt = DateTime.UtcNow;

        await _courseRepository.UpdateAsync(course);

        return (true, "Course deleted successfully");
    }
}
