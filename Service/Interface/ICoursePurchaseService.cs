
using Service.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interface;

public interface ICoursePurchaseService
{
    Task<(bool success, string message, CoursePurchaseDto? data)> PurchaseCourseAsync(int userId, int courseId);
    Task<(bool success, string message, List<CoursePurchaseDto>? data)> GetMyPurchasedCoursesAsync(int userId);
    Task<(bool success, string message)> CheckCoursePurchasedAsync(int userId, int courseId);
    Task<(bool success, string message, MyPurchasedCoursesDto? data)> GetMyPurchasesSummaryAsync(int userId);

   Task<CreatePaymentResponse> CreateCoursePaymentAsync(int studentId, int courseId);

    Task HandleWebhookAsync(PayOSWebhookRequest request);

    Task CancelPaymentAsync(long orderCode);
}
