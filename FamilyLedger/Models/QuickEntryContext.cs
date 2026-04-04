namespace FamilyLedger.Models;

public sealed class QuickEntryContext
{
    public IReadOnlyList<FamilyMember> Members { get; init; } = [];
    public IReadOnlyList<LedgerCategory> Categories { get; init; } = [];
    public IReadOnlyList<string> PaymentMethods { get; init; } = [];
}
