namespace FamilyLedger.Models;

public sealed class DashboardSnapshot
{
    public decimal MonthExpense { get; init; }
    public decimal MonthIncome { get; init; }
    public decimal NetBalance { get; init; }
    public int PendingCaptureCount { get; init; }
    public IReadOnlyList<TransactionListItem> RecentTransactions { get; init; } = [];
}
