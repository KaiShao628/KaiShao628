namespace FamilyLedger.Models;

public sealed class LedgerCategory
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public TransactionKind Kind { get; set; }
    public string Color { get; set; } = "#dc6e2f";
}
