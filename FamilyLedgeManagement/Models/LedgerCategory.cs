namespace FamilyLedgeManagement.Models;

/// <summary>
/// 家庭账单分类实体。
/// </summary>
public sealed class LedgerCategory
{
    /// <summary>
    /// 分类唯一标识。
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 分类名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 分类适用的收支类型。
    /// </summary>
    public TransactionKind Kind { get; set; }

    /// <summary>
    /// 分类主题色。
    /// </summary>
    public string Color { get; set; } = "#dc6e2f";
}
