using DatabaseCommon.EntityBase;
using FamilyLedgeManagement.Enums;

namespace FamilyLedgeManagement.Models;

/// <summary>
/// 家庭账单分类实体。
/// </summary>
public sealed class LedgerCategory : Entity
{
    /// <summary>
    /// 分类名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 分类适用的收支类型。
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// 分类主题色。
    /// </summary>
    public string Color { get; set; } = "#dc6e2f";
    public string Icon { get; internal set; }
}
