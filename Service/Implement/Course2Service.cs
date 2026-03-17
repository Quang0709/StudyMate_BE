using Repository.Interface;
using Repository.Model;
using Service.DTOs;
using Service.Interface;
using System;
using System.Threading.Tasks;

namespace Service.Implement;

public class Course2Service : ICourse2Service
{
    private readonly ICourseRepository _courseRepository;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IUnitOfWork _unitOfWork;

    public Course2Service(
        ICourseRepository courseRepository,
        ISubscriptionService subscriptionService,
        IUnitOfWork unitOfWork)
    {
        _courseRepository = courseRepository;
        _subscriptionService = subscriptionService;
        _unitOfWork = unitOfWork;
    }

    // ===== SEARCH =====
    public async Task<List<CourseSearchResultDto>> SearchAsync(SearchCourseDto dto)
    {
        var courses = await _courseRepository.SearchAsync(
            dto.Title,
            dto.CategoryId,
            dto.TeacherName
        );

        return courses.Select(c => new CourseSearchResultDto
        {
            Id = c.Id,
            Title = c.Title,
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
        var normalizedRole = role?.Trim().ToLower();

        if (normalizedRole != "student")
            return (false, "Only student can enroll");

        var course = await _courseRepository.GetByIdAsync(courseId);

        if (course == null || course.IsDeleted)
            return (false, "Course not found");

        // Get all students
        var students = await _unitOfWork.Students.GetAllAsync();
        var student = students.FirstOrDefault(s => s.UserId == userId);
        if (student == null)
            return (false, "Student profile not found");

        // Check if already enrolled
        var enrollments = await _unitOfWork.Enrollments.GetAllAsync();
        var existingEnrollment = enrollments.FirstOrDefault(e => e.StudentId == student.Id && e.CourseId == courseId);
        if (existingEnrollment != null)
            return (false, "You are already enrolled in this course");

        var hasPremium = await _subscriptionService
            .HasActiveSubscriptionAsync(userId, normalizedRole!);

        if (!hasPremium)
            return (false, "You need an active premium subscription to enroll in courses");

        // Create enrollment
        var enrollment = new Enrollment
        {
            StudentId = student.Id,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow
        };

        await _unitOfWork.Enrollments.AddAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Enrolled in course successfully");
    }

    public async Task<(bool success, bool isEnrolled, string message)> GetEnrollmentStatusAsync(int courseId, int userId, string role)
    {
        var normalizedRole = role?.Trim().ToLower();

        if (normalizedRole != "student")
            return (false, false, "Only student enrollment status is supported");

        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null || course.IsDeleted)
            return (false, false, "Course not found");

        var students = await _unitOfWork.Students.GetAllAsync();
        var student = students.FirstOrDefault(s => s.UserId == userId);
        if (student == null)
            return (false, false, "Student profile not found");

        var enrollments = await _unitOfWork.Enrollments.GetAllAsync();
        var isEnrolled = enrollments.Any(e => e.StudentId == student.Id && e.CourseId == courseId);

        return (true, isEnrolled, "Enrollment status retrieved successfully");
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