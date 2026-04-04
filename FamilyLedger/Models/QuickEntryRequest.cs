using System.ComponentModel.DataAnnotations;

namespace FamilyLedger.Models;

public sealed class QuickEntryRequest
{
    public TransactionKind Kind { get; set; } = TransactionKind.Expense;

    [Required]
    public string MemberId { get; set; } = string.Empty;

    [Required]
    public string CategoryId { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999")]
    public decimal Amount { get; set; }

    public string MerchantName { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = "Œ¢–≈÷ß∏∂";

    public string Note { get; set; } = string.Empty;

    public DateTimeOffset? OccurredAt { get; set; }
}
