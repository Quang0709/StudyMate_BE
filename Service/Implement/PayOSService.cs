using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using Repository.Interface;
using Repository.Model;
using Service.DTOs;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Implement;

public class PayOSService : IPayOSService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayOSClient _payOSClient;
    private readonly string _returnUrl;
    private readonly string _cancelUrl;

    public PayOSService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;

        _returnUrl = configuration["PayOS:ReturnUrl"]!;
        _cancelUrl = configuration["PayOS:CancelUrl"]!;

        _payOSClient = new PayOSClient(new PayOSOptions
        {
            ClientId = configuration["PayOS:ClientId"]!,
            ApiKey = configuration["PayOS:ApiKey"]!,
            ChecksumKey = configuration["PayOS:ChecksumKey"]!
        });
    }

    public async Task<(bool success, string message, PaymentLinkDto? data)> CreatePaymentLinkAsync(
        int userId, string orderType, int referenceId, decimal amount, string description)
    {
        try
        {
            // Generate unique orderCode dùng timestamp ms (??m b?o unique & là s? d??ng)
            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // L?u pending PaymentOrder vào DB tr??c khi g?i PayOS
            var paymentOrder = new PaymentOrder
            {
                OrderCode = orderCode,
                UserId = userId,
                OrderType = orderType,
                ReferenceId = referenceId,
                Amount = amount,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.PaymentOrders.AddAsync(paymentOrder);
            await _unitOfWork.SaveChangesAsync();

            // T?o payment link v?i PayOS v2
            var paymentData = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = (int)amount,
                Description = description,
                Items = new List<PaymentLinkItem>
                {
                    new PaymentLinkItem { Name = description, Quantity = 1, Price = (int)amount }
                },
                ReturnUrl = _returnUrl,
                CancelUrl = _cancelUrl
            };

            var createPaymentResult = await _payOSClient.PaymentRequests.CreateAsync(paymentData);

            return (true, "Payment link created successfully", new PaymentLinkDto
            {
                CheckoutUrl = createPaymentResult.CheckoutUrl,
                OrderCode = orderCode,
                Amount = amount,
                Description = description
            });
        }
        catch (Exception ex)
        {
            return (false, $"Error creating payment link: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message)> HandleWebhookAsync(PayOSWebhookDto webhook)
    {
        try
        {
            // Verify webhook signature v?i PayOS SDK
            WebhookData verifiedData;
            try
            {
                var webhookPayload = new Webhook
                {
                    Code = webhook.Code,
                    Description = webhook.Desc,
                    Success = webhook.Success,
                    Data = webhook.Data != null ? new WebhookData
                    {
                        OrderCode = webhook.Data.OrderCode,
                        Amount = (int)webhook.Data.Amount,
                        Description = webhook.Data.Description ?? string.Empty,
                        AccountNumber = webhook.Data.AccountNumber ?? string.Empty,
                        Reference = webhook.Data.Reference ?? string.Empty,
                        TransactionDateTime = webhook.Data.TransactionDateTime ?? string.Empty,
                        Currency = webhook.Data.Currency ?? string.Empty,
                        PaymentLinkId = webhook.Data.PaymentLinkId ?? string.Empty,
                        Code = webhook.Data.Code ?? string.Empty,
                        CounterAccountBankId = webhook.Data.CounterAccountBankId,
                        CounterAccountBankName = webhook.Data.CounterAccountBankName,
                        CounterAccountName = webhook.Data.CounterAccountName,
                        CounterAccountNumber = webhook.Data.CounterAccountNumber,
                        VirtualAccountName = webhook.Data.VirtualAccountName,
                        VirtualAccountNumber = webhook.Data.VirtualAccountNumber
                    } : new WebhookData(),
                    Signature = webhook.Signature ?? string.Empty
                };

                verifiedData = await _payOSClient.Webhooks.VerifyAsync(webhookPayload);
            }
            catch
            {
                return (false, "Invalid webhook signature");
            }

            // Ch? x? lý khi payment thành công (code == "00")
            if (webhook.Code != "00" || !webhook.Success)
            {
                return (true, "Webhook received but payment not successful, ignored");
            }

            var orderCode = verifiedData.OrderCode;

            // Tìm PaymentOrder trong DB
            var paymentOrder = await _unitOfWork.PaymentOrders.FirstOrDefaultAsync(
                p => p.OrderCode == orderCode && p.Status == "pending");

            if (paymentOrder == null)
            {
                // Có th? ?ã ???c x? lý r?i (idempotency)
                return (true, "Order not found or already processed");
            }

            // Update payment order status
            paymentOrder.Status = "paid";
            paymentOrder.PayOSTransactionId = verifiedData.Reference;
            paymentOrder.PaidAt = DateTime.UtcNow;
            _unitOfWork.PaymentOrders.Update(paymentOrder);

            // X? lý logic nghi?p v? t??ng ?ng
            if (paymentOrder.OrderType == "course_purchase")
            {
                await CompleteCoursePaymentAsync(paymentOrder);
            }
            else if (paymentOrder.OrderType == "subscription")
            {
                await CompleteSubscriptionPaymentAsync(paymentOrder);
            }

            await _unitOfWork.SaveChangesAsync();

            return (true, "Webhook processed successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error processing webhook: {ex.Message}");
        }
    }

    public async Task<(bool success, string message, string? status)> VerifyPaymentAsync(long orderCode)
    {
        try
        {
            var paymentLinkInfo = await _payOSClient.PaymentRequests.GetAsync(orderCode);
            return (true, "Payment status retrieved", paymentLinkInfo.Status.ToString());
        }
        catch (Exception ex)
        {
            return (false, $"Error verifying payment: {ex.Message}", null);
        }
    }

    // ===================== PRIVATE HELPERS =====================

    private async Task CompleteCoursePaymentAsync(PaymentOrder paymentOrder)
    {
        var courseId = paymentOrder.ReferenceId;

        var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.UserId == paymentOrder.UserId);
        if (student == null) return;

        // Ki?m tra ch?a mua (idempotency)
        var existingPurchase = await _unitOfWork.CoursePurchases.FirstOrDefaultAsync(
            cp => cp.StudentId == student.Id && cp.CourseId == courseId);
        if (existingPurchase != null) return;

        var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
        if (course == null) return;

        // T?o CoursePurchase v?i status completed
        await _unitOfWork.CoursePurchases.AddAsync(new CoursePurchase
        {
            StudentId = student.Id,
            CourseId = courseId,
            PurchasePrice = paymentOrder.Amount,
            PurchaseDate = DateTime.UtcNow,
            PaymentStatus = "completed",
            TransactionId = paymentOrder.PayOSTransactionId ?? paymentOrder.OrderCode.ToString()
        });

        // T? ??ng enroll student vào course
        var existingEnrollment = await _unitOfWork.Enrollments.FirstOrDefaultAsync(
            e => e.StudentId == student.Id && e.CourseId == courseId);
        if (existingEnrollment == null)
        {
            await _unitOfWork.Enrollments.AddAsync(new Enrollment
            {
                StudentId = student.Id,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow
            });
        }
    }

    private async Task CompleteSubscriptionPaymentAsync(PaymentOrder paymentOrder)
    {
        var planId = paymentOrder.ReferenceId;
        var plan = await _unitOfWork.PremiumPlans.GetByIdAsync(planId);
        if (plan == null) return;

        int? studentId = null;
        int? teacherId = null;

        if (plan.TargetRole == "student")
        {
            var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.UserId == paymentOrder.UserId);
            if (student == null) return;
            studentId = student.Id;

            // Idempotency check
            var existing = await _unitOfWork.UserSubscriptions.FirstOrDefaultAsync(
                s => s.StudentId == studentId && s.IsActive);
            if (existing != null) return;
        }
        else if (plan.TargetRole == "teacher")
        {
            var teacher = await _unitOfWork.Teachers.FirstOrDefaultAsync(t => t.UserId == paymentOrder.UserId);
            if (teacher == null) return;
            teacherId = teacher.Id;

            var existing = await _unitOfWork.UserSubscriptions.FirstOrDefaultAsync(
                s => s.TeacherId == teacherId && s.IsActive);
            if (existing != null) return;
        }

        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(plan.DurationDays ?? 30);

        await _unitOfWork.UserSubscriptions.AddAsync(new UserSubscription
        {
            StudentId = studentId,
            TeacherId = teacherId,
            PlanId = planId,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true
        });
    }
}
