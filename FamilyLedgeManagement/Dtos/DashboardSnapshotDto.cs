namespace FamilyLedgeManagement.Dtos;

/// <summary>
/// 首页概览数据 DTO。
/// </summary>
public sealed class DashboardSnapshotDto
{
    /// <summary>
    /// 当前月份支出总额。
    /// </summary>
    public decimal MonthExpense { get; set; }

    /// <summary>
    /// 当前月份收入总额。
    /// </summary>
    public decimal MonthIncome { get; set; }

    /// <summary>
    /// 当前月份结余。
    /// </summary>
    public decimal NetBalance { get; set; }

    /// <summary>
    /// 最近账单列表。
    /// </summary>
    public IReadOnlyList<TransactionListItemDto> RecentTransactions { get; set; } = [];
}
