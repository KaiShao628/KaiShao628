using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Dtos;

public sealed class LedgerCategoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TransactionKind Kind { get; set; }
}

