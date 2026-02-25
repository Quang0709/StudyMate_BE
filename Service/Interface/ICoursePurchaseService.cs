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
}
