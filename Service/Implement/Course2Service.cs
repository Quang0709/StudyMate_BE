using Repository.Interface;
using Repository.Model;
using Service.DTOs;
using Service.Interface;

namespace Service.Implement;

public class Course2Service : ICourse2Service
{
    private readonly ICourseRepository _courseRepository;
    private readonly ISubscriptionService _subscriptionService;

    public Course2Service(
        ICourseRepository courseRepository,
        ISubscriptionService subscriptionService)
    {
        _courseRepository = courseRepository;
        _subscriptionService = subscriptionService;
    }

    // ===== SEARCH =====
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

    // ===== SOFT DELETE =====
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

    // ===== ENROLL WITH PREMIUM CHECK =====
    public async Task<(bool success, string message)>
        EnrollCourseAsync(int courseId, int userId, string role)
    {
        if (role != "STUDENT")
            return (false, "Only student can enroll");

        var course = await _courseRepository.GetByIdAsync(courseId);

        if (course == null || course.IsDeleted)
            return (false, "Course not found");

        var hasPremium = await _subscriptionService
            .HasActiveSubscriptionAsync(userId, role);

        if (!hasPremium)
            return (false, "Your premium package has expired");

        return (true, "Enroll successfully");
    }


    public async Task<(bool success, string message, Course? course)>
    GetCourseDetailAsync(int courseId, int userId, string role)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);

        if (course == null || course.IsDeleted)
            return (false, "Course not found", null);

        // Nếu không phải student → cho truy cập
        if (role != "STUDENT")
            return (true, "Access granted", course);

        // Nếu là student → phải có premium
        var hasPremium = await _subscriptionService
            .HasActiveSubscriptionAsync(userId, role);

        if (!hasPremium)
            return (false, "You need premium package to access this course", null);

        return (true, "Access granted", course);
    }
}