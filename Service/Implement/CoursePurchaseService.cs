using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Repository.Model;
using Service.DTOs;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayOS;

namespace Service.Implement;

public class CoursePurchaseService : ICoursePurchaseService
{
    private readonly IUnitOfWork _unitOfWork;

    public CoursePurchaseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // =========================================================
    // 1️⃣ TẠO LINK THANH TOÁN
    // =========================================================
    public async Task<CreatePaymentResponse> CreateCoursePaymentAsync(int userId, int courseId)
    {
        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
            throw new Exception("Student profile not found");

        var course = await _unitOfWork.Courses.GetByIdAsync(courseId);

        if (course == null || course.IsDeleted)
            throw new Exception("Course not found");

        if (course.Price == null || course.Price <= 0)
            throw new Exception("Course is free");

        var existed = await _unitOfWork.CoursePurchases
            .FirstOrDefaultAsync(x =>
                x.StudentId == student.Id &&
                x.CourseId == courseId &&
                x.PaymentStatus == "completed");

        if (existed != null)
            throw new Exception("Course already purchased");

        var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var purchase = new CoursePurchase
        {
            StudentId = student.Id,
            CourseId = courseId,
            PurchasePrice = course.Price.Value,
            PurchaseDate = DateTime.UtcNow,
            PaymentStatus = "pending",
            TransactionId = orderCode.ToString()
        };

        await _unitOfWork.CoursePurchases.AddAsync(purchase);
        await _unitOfWork.SaveChangesAsync();

        // Giả lập link thanh toán (vì PayOSClient của bạn không có method kia)
        var checkoutUrl = $"https://yourdomain.com/payment/{orderCode}";

        return new CreatePaymentResponse
        {
            CheckoutUrl = checkoutUrl,
            PaymentLinkId = orderCode
        };
    }

    // =========================================================
    // 2️⃣ HỦY LINK THANH TOÁN
    // =========================================================
    public async Task CancelPaymentAsync(long paymentLinkId)
    {
        var purchase = await _unitOfWork.CoursePurchases
            .FirstOrDefaultAsync(x => x.TransactionId == paymentLinkId.ToString());

        if (purchase == null)
            throw new Exception("Payment not found");

        if (purchase.PaymentStatus == "completed")
            throw new Exception("Cannot cancel completed payment");

        purchase.PaymentStatus = "cancelled";

        _unitOfWork.CoursePurchases.Update(purchase);
        await _unitOfWork.SaveChangesAsync();
    }

    // =========================================================
    // 3️⃣ MUA KHÓA HỌC (SAU KHI PAYMENT SUCCESS)
    // =========================================================
    public async Task<(bool success, string message, CoursePurchaseDto? data)>
        PurchaseCourseAsync(int userId, int courseId)
    {
        try
        {
            var student = await _unitOfWork.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                return (false, "Student not found", null);

            var purchase = await _unitOfWork.CoursePurchases
                .FirstOrDefaultAsync(x =>
                    x.StudentId == student.Id &&
                    x.CourseId == courseId &&
                    x.PaymentStatus == "pending");

            if (purchase == null)
                return (false, "No pending payment found", null);

            purchase.PaymentStatus = "completed";

            _unitOfWork.CoursePurchases.Update(purchase);

            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow
            };

            await _unitOfWork.Enrollments.AddAsync(enrollment);
            await _unitOfWork.SaveChangesAsync();

            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);

            var dto = new CoursePurchaseDto
            {
                Id = purchase.Id,
                StudentId = purchase.StudentId,
                CourseId = purchase.CourseId,
                CourseTitle = course?.Title,
                CourseDescription = course?.Description,
                PurchasePrice = purchase.PurchasePrice,
                PurchaseDate = purchase.PurchaseDate,
                PaymentStatus = purchase.PaymentStatus,
                TransactionId = purchase.TransactionId
            };

            return (true, "Course purchased successfully", dto);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    // =========================================================
    // 4️⃣ CHECK ĐÃ MUA CHƯA
    // =========================================================
    public async Task<(bool success, string message)>
        CheckCoursePurchasedAsync(int userId, int courseId)
    {
        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
            return (false, "Student not found");

        var purchase = await _unitOfWork.CoursePurchases
            .FirstOrDefaultAsync(x =>
                x.StudentId == student.Id &&
                x.CourseId == courseId &&
                x.PaymentStatus == "completed");

        if (purchase != null)
            return (true, "Course already purchased");

        return (false, "Course not purchased");
    }

    // =========================================================
    // 5️⃣ LẤY DANH SÁCH ĐÃ MUA
    // =========================================================
    public async Task<(bool success, string message, List<CoursePurchaseDto>? data)>
        GetMyPurchasedCoursesAsync(int userId)
    {
        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
            return (false, "Student not found", null);

        var purchases = await _unitOfWork.CoursePurchases
            .FindAsync(x =>
                x.StudentId == student.Id &&
                x.PaymentStatus == "completed");

        var result = purchases.Select(p => new CoursePurchaseDto
        {
            Id = p.Id,
            StudentId = p.StudentId,
            CourseId = p.CourseId,
            PurchasePrice = p.PurchasePrice,
            PurchaseDate = p.PurchaseDate,
            PaymentStatus = p.PaymentStatus,
            TransactionId = p.TransactionId
        }).ToList();

        return (true, "Success", result);
    }

    // =========================================================
    // 6️⃣ SUMMARY
    // =========================================================
    public async Task<(bool success, string message, MyPurchasedCoursesDto? data)>
        GetMyPurchasesSummaryAsync(int userId)
    {
        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
            return (false, "Student not found", null);

        var purchases = await _unitOfWork.CoursePurchases
            .FindAsync(x =>
                x.StudentId == student.Id &&
                x.PaymentStatus == "completed");

        var list = purchases.ToList();

        var summary = new MyPurchasedCoursesDto
        {
            TotalPurchased = list.Count,
            TotalSpent = list.Sum(x => x.PurchasePrice),
            Purchases = list.Select(p => new CoursePurchaseDto
            {
                Id = p.Id,
                CourseId = p.CourseId,
                PurchasePrice = p.PurchasePrice,
                PurchaseDate = p.PurchaseDate,
                PaymentStatus = p.PaymentStatus,
                TransactionId = p.TransactionId
            }).ToList()
        };

        return (true, "Success", summary);
    }

    public async Task HandleWebhookAsync(PayOSWebhookRequest request)
    {
        if (request == null)
            throw new Exception("Invalid webhook request");

        var orderCode = request.OrderCode.ToString();

        var purchase = await _unitOfWork.CoursePurchases
            .FirstOrDefaultAsync(x => x.TransactionId == orderCode);

        if (purchase == null)
            throw new Exception("Purchase not found");

        // Nếu thanh toán thành công
        if (request.Status == "PAID")
        {
            purchase.PaymentStatus = "completed";

            _unitOfWork.CoursePurchases.Update(purchase);

            // Tạo enrollment
            var enrollment = new Enrollment
            {
                StudentId = purchase.StudentId,
                CourseId = purchase.CourseId,
                EnrolledAt = DateTime.UtcNow
            };

            await _unitOfWork.Enrollments.AddAsync(enrollment);
        }
        else if (request.Status == "CANCELLED")
        {
            purchase.PaymentStatus = "cancelled";
            _unitOfWork.CoursePurchases.Update(purchase);
        }
        else if (request.Status == "FAILED")
        {
            purchase.PaymentStatus = "failed";
            _unitOfWork.CoursePurchases.Update(purchase);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}