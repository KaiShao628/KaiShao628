namespace FamilyLedgeManagement.Dtos;

public sealed class DashboardSnapshotDto
{
    public decimal MonthExpense { get; set; }
    public decimal MonthIncome { get; set; }
    public decimal NetBalance { get; set; }
    public int PendingCaptureCount { get; set; }
    public IReadOnlyList<TransactionListItemDto> RecentTransactions { get; set; } = [];
}

