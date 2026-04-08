using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Dtos;

public sealed class TransactionListItemDto
{
    public string Id { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public TransactionKind Kind { get; set; }
    public decimal Amount { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
}

