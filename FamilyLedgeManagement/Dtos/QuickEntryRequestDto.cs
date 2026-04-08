using System.ComponentModel.DataAnnotations;
using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Dtos;

public sealed class QuickEntryRequestDto
{
    public TransactionKind Kind { get; set; } = TransactionKind.Expense;

    [Required]
    public string MemberId { get; set; } = string.Empty;

    [Required]
    public string CategoryId { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999")]
    public decimal Amount { get; set; }

    public string MerchantName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "微信支付";
    public string Note { get; set; } = string.Empty;
    public DateTimeOffset? OccurredAt { get; set; }
}

