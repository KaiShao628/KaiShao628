namespace FamilyLedger.Models;

public sealed class LedgerTransaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string MemberId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public TransactionKind Kind { get; set; }
    public decimal Amount { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
}
