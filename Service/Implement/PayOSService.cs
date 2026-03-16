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
using System.Linq;
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
            if (paymentOrder.OrderType == "subscription")
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

    public async Task<SyncPaymentResultDto> SyncPaymentStatusAsync()
    {
        var result = new SyncPaymentResultDto();

        // Sync c? pending, cancelled, expired — b? qua paid ?ã xong
        var ordersToCheck = (await _unitOfWork.PaymentOrders.FindAsync(
            o => o.Status == "pending" || o.Status == "cancelled" || o.Status == "expired")).ToList();

        result.TotalChecked = ordersToCheck.Count;

        foreach (var order in ordersToCheck)
        {
            try
            {
                var paymentLinkInfo = await _payOSClient.PaymentRequests.GetAsync(order.OrderCode);
                var payosStatus = paymentLinkInfo.Status.ToString().ToUpper();

                if (payosStatus == "PAID")
                {
                    var previousStatus = order.Status;
                    order.Status = "paid";
                    order.PaidAt = order.PaidAt ?? DateTime.UtcNow;

                    var transactions = paymentLinkInfo.Transactions;
                    if (transactions != null && transactions.Count > 0)
                        order.PayOSTransactionId = transactions[0].Reference;

                    _unitOfWork.PaymentOrders.Update(order);

                    if (order.OrderType == "subscription")
                        await CompleteSubscriptionPaymentAsync(order);

                    result.UpdatedToPaid++;
                    result.Details.Add($"OrderCode {order.OrderCode}: {previousStatus} ? paid");
                }
                else if (payosStatus == "CANCELLED" && order.Status != "cancelled")
                {
                    var previousStatus = order.Status;
                    order.Status = "cancelled";
                    _unitOfWork.PaymentOrders.Update(order);
                    result.UpdatedToCancelled++;
                    result.Details.Add($"OrderCode {order.OrderCode}: {previousStatus} ? cancelled");
                }
                else if (payosStatus == "EXPIRED" && order.Status != "expired")
                {
                    var previousStatus = order.Status;
                    order.Status = "expired";
                    _unitOfWork.PaymentOrders.Update(order);
                    result.UpdatedToExpired++;
                    result.Details.Add($"OrderCode {order.OrderCode}: {previousStatus} ? expired");
                }
                else
                {
                    result.Details.Add($"OrderCode {order.OrderCode}: no change (PayOS={payosStatus}, DB={order.Status})");
                }
            }
            catch (Exception ex)
            {
                result.Details.Add($"OrderCode {order.OrderCode}: error - {ex.Message}");
            }
        }

        if (result.UpdatedToPaid + result.UpdatedToCancelled + result.UpdatedToExpired > 0)
            await _unitOfWork.SaveChangesAsync();

        return result;
    }

    public async Task<PaymentOrderListDto> GetPaymentOrdersAsync(PaymentFilterDto filter)
    {
        var allOrders = (await _unitOfWork.PaymentOrders.GetAllAsync()).ToList();
        var allUsers = (await _unitOfWork.Users.GetAllAsync()).ToList();

        var query = allOrders.Join(allUsers,
            o => o.UserId,
            u => u.Id,
            (o, u) => new PaymentOrderDetailDto
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                UserId = o.UserId,
                UserEmail = u.Email,
                UserFullName = u.FullName,
                OrderType = o.OrderType,
                ReferenceId = o.ReferenceId,
                Amount = o.Amount,
                Status = o.Status,
                PayOSTransactionId = o.PayOSTransactionId,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt
            }).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(x => x.Status == filter.Status);

        if (!string.IsNullOrWhiteSpace(filter.OrderType))
            query = query.Where(x => x.OrderType == filter.OrderType);

        if (filter.FromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(x => x.CreatedAt <= filter.ToDate.Value.AddDays(1));

        if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
        {
            var kw = filter.SearchKeyword.ToLower();
            query = query.Where(x =>
                x.UserEmail.ToLower().Contains(kw) ||
                x.UserFullName.ToLower().Contains(kw) ||
                x.OrderCode.ToString().Contains(kw));
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        var items = query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return new PaymentOrderListDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages
        };
    }

    public async Task<PaymentOrderDetailDto?> GetPaymentOrderByIdAsync(int id)
    {
        var order = await _unitOfWork.PaymentOrders.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return null;

        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == order.UserId);
        if (user == null) return null;

        return new PaymentOrderDetailDto
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            UserId = order.UserId,
            UserEmail = user.Email,
            UserFullName = user.FullName,
            OrderType = order.OrderType,
            ReferenceId = order.ReferenceId,
            Amount = order.Amount,
            Status = order.Status,
            PayOSTransactionId = order.PayOSTransactionId,
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt
        };
    }

    public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var allOrders = (await _unitOfWork.PaymentOrders.GetAllAsync()).ToList();
        var allPaid = allOrders.Where(o => o.Status == "paid").ToList();

        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);
        var lastMonthEnd = thisMonthStart.AddTicks(-1);

        var revenueThisMonth = allPaid
            .Where(o => o.PaidAt >= thisMonthStart)
            .Sum(o => o.Amount);

        var revenueLastMonth = allPaid
            .Where(o => o.PaidAt >= lastMonthStart && o.PaidAt <= lastMonthEnd)
            .Sum(o => o.Amount);

        var growthPercent = revenueLastMonth == 0
            ? (revenueThisMonth > 0 ? 100m : 0m)
            : Math.Round((revenueThisMonth - revenueLastMonth) / revenueLastMonth * 100, 2);

        var revenueByMonth = allPaid
            .Where(o => o.PaidAt.HasValue && o.PaidAt.Value.Year == targetYear)
            .GroupBy(o => new { o.PaidAt!.Value.Year, o.PaidAt.Value.Month })
            .Select(g => new RevenueByMonthDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalRevenue = g.Sum(o => o.Amount),
                TransactionCount = g.Count()
            })
            .OrderBy(x => x.Month)
            .ToList();

        var revenueByType = allPaid
            .GroupBy(o => o.OrderType)
            .Select(g => new RevenueByTypeDto
            {
                OrderType = g.Key,
                TotalRevenue = g.Sum(o => o.Amount),
                TransactionCount = g.Count()
            })
            .ToList();

        return new RevenueSummaryDto
        {
            TotalRevenue = allPaid.Sum(o => o.Amount),
            RevenueThisMonth = revenueThisMonth,
            RevenueLastMonth = revenueLastMonth,
            MonthOverMonthGrowthPercent = growthPercent,
            TotalTransactions = allOrders.Count,
            PaidTransactions = allPaid.Count,
            PendingTransactions = allOrders.Count(o => o.Status == "pending"),
            RevenueByMonth = revenueByMonth,
            RevenueByType = revenueByType
        };
    }

    // ===================== PRIVATE HELPERS =====================

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
