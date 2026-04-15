using System.ComponentModel.DataAnnotations;
using FamilyLedgeManagement.Enums;

namespace FamilyLedgeManagement.Dtos;

/// <summary>
/// 账单分类新增和编辑 DTO。
/// </summary>
public sealed class LedgerCategoryEditorDto
{
    /// <summary>
    /// 分类唯一标识，新建时为空。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 分类名称。
    /// </summary>
    [Required(ErrorMessage = "请输入分类名称。")]
    [StringLength(20, ErrorMessage = "分类名称不能超过 20 个字符。")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 分类收支类型。
    /// </summary>
    [Required(ErrorMessage = "请选择收支类型。")]
    public TransactionKind Kind { get; set; } = TransactionKind.Expense;

    /// <summary>
    /// 分类主题色，使用 #RRGGBB 格式。
    /// </summary>
    [Required(ErrorMessage = "请输入分类色。")]
    [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "颜色需要使用 #RRGGBB 格式。")]
    public string Color { get; set; } = "#dc6e2f";
}
