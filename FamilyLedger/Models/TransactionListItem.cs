namespace FamilyLedger.Models;

public sealed class TransactionListItem
{
    public string Id { get; init; } = string.Empty;
    public string MemberName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public TransactionKind Kind { get; init; }
    public decimal Amount { get; init; }
    public string MerchantName { get; init; } = string.Empty;
    public string PaymentMethod { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
}
