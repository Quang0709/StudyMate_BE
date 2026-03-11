using System;
using System.Collections.Generic;

namespace Service.DTOs;

public class PaymentLinkDto
{
    public string CheckoutUrl { get; set; } = null!;
    public long OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
}

public class CreateCoursePaymentDto
{
    public int CourseId { get; set; }
}

public class CreateSubscriptionPaymentDto
{
    public int PlanId { get; set; }
}

public class PayOSWebhookDto
{
    public string Code { get; set; } = null!;
    public string Desc { get; set; } = null!;
    public bool Success { get; set; }
    public PayOSWebhookData? Data { get; set; }
    public string? Signature { get; set; }
}

public class PayOSWebhookData
{
    public long OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? AccountNumber { get; set; }
    public string? Reference { get; set; }
    public string? TransactionDateTime { get; set; }
    public string? Currency { get; set; }
    public string? PaymentLinkId { get; set; }
    public string? Code { get; set; }
    public string? Desc { get; set; }
    public string? CounterAccountBankId { get; set; }
    public string? CounterAccountBankName { get; set; }
    public string? CounterAccountName { get; set; }
    public string? CounterAccountNumber { get; set; }
    public string? VirtualAccountName { get; set; }
    public string? VirtualAccountNumber { get; set; }
}

// ===================== ADMIN DASHBOARD DTOs =====================

public class PaymentOrderDetailDto
{
    public int Id { get; set; }
    public long OrderCode { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = null!;
    public string UserFullName { get; set; } = null!;
    public string OrderType { get; set; } = null!;
    public int ReferenceId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = null!;
    public string? PayOSTransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class PaymentOrderListDto
{
    public List<PaymentOrderDetailDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PaymentFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    /// <summary>"pending" | "paid" | "cancelled" | "expired" | null (all)</summary>
    public string? Status { get; set; }
    /// <summary>"course_purchase" | "subscription" | null (all)</summary>
    public string? OrderType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchKeyword { get; set; }
}

public class RevenueByMonthDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TransactionCount { get; set; }
}

public class RevenueByTypeDto
{
    public string OrderType { get; set; } = null!;
    public decimal TotalRevenue { get; set; }
    public int TransactionCount { get; set; }
}

public class RevenueSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }
    public decimal MonthOverMonthGrowthPercent { get; set; }
    public int TotalTransactions { get; set; }
    public int PaidTransactions { get; set; }
    public int PendingTransactions { get; set; }
    public List<RevenueByMonthDto> RevenueByMonth { get; set; } = new();
    public List<RevenueByTypeDto> RevenueByType { get; set; } = new();
}

public class SyncPaymentResultDto
{
    public int TotalChecked { get; set; }
    public int UpdatedToPaid { get; set; }
    public int UpdatedToCancelled { get; set; }
    public int UpdatedToExpired { get; set; }
    public List<string> Details { get; set; } = new();
}
