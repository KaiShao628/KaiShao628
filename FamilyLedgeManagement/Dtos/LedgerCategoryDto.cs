using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Dtos;

/// <summary>
/// 账单分类只读展示 DTO。
/// </summary>
public sealed class LedgerCategoryDto
{
    /// <summary>
    /// 分类唯一标识。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 分类名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 分类收支类型。
    /// </summary>
    public TransactionKind Kind { get; set; }

    /// <summary>
    /// 分类主题色。
    /// </summary>
    public string Color { get; set; } = "#dc6e2f";
}
