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

/// <summary>
/// Webhook data t? PayOS g?i v?
/// </summary>
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
