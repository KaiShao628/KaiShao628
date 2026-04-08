namespace FamilyLedgeManagement.Dtos;

public sealed class QuickEntryContextDto
{
    public IReadOnlyList<FamilyMemberDto> Members { get; set; } = [];
    public IReadOnlyList<LedgerCategoryDto> Categories { get; set; } = [];
    public IReadOnlyList<string> PaymentMethods { get; set; } = [];
}

