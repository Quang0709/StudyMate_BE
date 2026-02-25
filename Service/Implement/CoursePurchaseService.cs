using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Repository.Model;
using Service.DTOs;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Implement;

public class CoursePurchaseService : ICoursePurchaseService
{
    private readonly IUnitOfWork _unitOfWork;

    public CoursePurchaseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool success, string message, CoursePurchaseDto? data)> PurchaseCourseAsync(int userId, int courseId)
    {
        try
        {
            // Get student profile
            var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null)
            {
                return (false, "Student profile not found. Only students can purchase courses.", null);
            }

            // Get course
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
            {
                return (false, "Course not found", null);
            }

            // Check if course is deleted
            if (course.IsDeleted)
            {
                return (false, "This course is no longer available", null);
            }

            // Check if course is approved
            if (course.Status != "paid")
            {
                return (false, "This course is not available for purchase yet", null);
            }

            // Check if course has price (free courses don't need purchase)
            if (course.Price == null || course.Price <= 0)
            {
                return (false, "This course is free. You can enroll directly without purchasing.", null);
            }

            // Check if student already purchased this course
            var existingPurchase = await _unitOfWork.CoursePurchases.FirstOrDefaultAsync(
                cp => cp.StudentId == student.Id && cp.CourseId == courseId);

            if (existingPurchase != null)
            {
                return (false, "You have already purchased this course", null);
            }

            // Create purchase record
            var purchase = new CoursePurchase
            {
                StudentId = student.Id,
                CourseId = courseId,
                PurchasePrice = course.Price.Value,
                PurchaseDate = DateTime.UtcNow,
                PaymentStatus = "completed",  // Auto completed (no real payment for now)
                TransactionId = GenerateTransactionId()
            };

            await _unitOfWork.CoursePurchases.AddAsync(purchase);

            // Auto enroll student to course after purchase
            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow
            };

            await _unitOfWork.Enrollments.AddAsync(enrollment);
            await _unitOfWork.SaveChangesAsync();

            // Get teacher info for response
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(course.TeacherId);
            var teacherUser = teacher != null ? await _unitOfWork.Users.GetByIdAsync(teacher.UserId) : null;

            var purchaseDto = new CoursePurchaseDto
            {
                Id = purchase.Id,
                StudentId = purchase.StudentId,
                CourseId = purchase.CourseId,
                CourseTitle = course.Title,
                CourseDescription = course.Description,
                TeacherName = teacherUser?.FullName,
                PurchasePrice = purchase.PurchasePrice,
                PurchaseDate = purchase.PurchaseDate,
                PaymentStatus = purchase.PaymentStatus,
                TransactionId = purchase.TransactionId
            };

            return (true, "Course purchased successfully! You now have lifetime access to this course.", purchaseDto);
        }
        catch (Exception ex)
        {
            return (false, $"Error purchasing course: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message, List<CoursePurchaseDto>? data)> GetMyPurchasedCoursesAsync(int userId)
    {
        try
        {
            // Get student profile
            var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null)
            {
                return (false, "Student profile not found", null);
            }

            // Get all purchases
            var purchases = await _unitOfWork.CoursePurchases.FindAsync(cp => cp.StudentId == student.Id);

            var purchaseDtos = new List<CoursePurchaseDto>();

            foreach (var purchase in purchases)
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(purchase.CourseId);
                if (course == null) continue;

                var teacher = await _unitOfWork.Teachers.GetByIdAsync(course.TeacherId);
                var teacherUser = teacher != null ? await _unitOfWork.Users.GetByIdAsync(teacher.UserId) : null;

                purchaseDtos.Add(new CoursePurchaseDto
                {
                    Id = purchase.Id,
                    StudentId = purchase.StudentId,
                    CourseId = purchase.CourseId,
                    CourseTitle = course.Title,
                    CourseDescription = course.Description,
                    TeacherName = teacherUser?.FullName,
                    PurchasePrice = purchase.PurchasePrice,
                    PurchaseDate = purchase.PurchaseDate,
                    PaymentStatus = purchase.PaymentStatus,
                    TransactionId = purchase.TransactionId
                });
            }

            return (true, "Purchased courses retrieved successfully", purchaseDtos.OrderByDescending(p => p.PurchaseDate).ToList());
        }
        catch (Exception ex)
        {
            return (false, $"Error retrieving purchased courses: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message)> CheckCoursePurchasedAsync(int userId, int courseId)
    {
        try
        {
            // Get student profile
            var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null)
            {
                return (false, "Student profile not found");
            }

            // Check if purchased
            var purchase = await _unitOfWork.CoursePurchases.FirstOrDefaultAsync(
                cp => cp.StudentId == student.Id && cp.CourseId == courseId);

            if (purchase != null)
            {
                return (true, "Course already purchased");
            }

            return (false, "Course not purchased yet");
        }
        catch (Exception ex)
        {
            return (false, $"Error checking course purchase: {ex.Message}");
        }
    }

    public async Task<(bool success, string message, MyPurchasedCoursesDto? data)> GetMyPurchasesSummaryAsync(int userId)
    {
        try
        {
            // Get student profile
            var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null)
            {
                return (false, "Student profile not found", null);
            }

            // Get all purchases
            var purchases = await _unitOfWork.CoursePurchases.FindAsync(cp => cp.StudentId == student.Id);
            var purchasesList = purchases.ToList();

            var purchaseDtos = new List<CoursePurchaseDto>();

            foreach (var purchase in purchasesList)
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(purchase.CourseId);
                if (course == null) continue;

                var teacher = await _unitOfWork.Teachers.GetByIdAsync(course.TeacherId);
                var teacherUser = teacher != null ? await _unitOfWork.Users.GetByIdAsync(teacher.UserId) : null;

                purchaseDtos.Add(new CoursePurchaseDto
                {
                    Id = purchase.Id,
                    StudentId = purchase.StudentId,
                    CourseId = purchase.CourseId,
                    CourseTitle = course.Title,
                    CourseDescription = course.Description,
                    TeacherName = teacherUser?.FullName,
                    PurchasePrice = purchase.PurchasePrice,
                    PurchaseDate = purchase.PurchaseDate,
                    PaymentStatus = purchase.PaymentStatus,
                    TransactionId = purchase.TransactionId
                });
            }

            var summary = new MyPurchasedCoursesDto
            {
                TotalPurchased = purchasesList.Count,
                TotalSpent = purchasesList.Sum(p => p.PurchasePrice),
                Purchases = purchaseDtos.OrderByDescending(p => p.PurchaseDate).ToList()
            };

            return (true, "Purchase summary retrieved successfully", summary);
        }
        catch (Exception ex)
        {
            return (false, $"Error retrieving purchase summary: {ex.Message}", null);
        }
    }

    private string GenerateTransactionId()
    {
        // Format: TXN + yyyyMMddHHmmss + random 4 digits
        return $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
    }
}
