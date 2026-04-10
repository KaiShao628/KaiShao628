using System.ComponentModel.DataAnnotations;
using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Dtos;

/// <summary>
/// 账单新增和编辑 DTO。
/// </summary>
public sealed class TransactionEditorDto
{
    /// <summary>
    /// 账单唯一标识，新建时为空。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 账单归属成员标识。
    /// </summary>
    [Required(ErrorMessage = "请选择成员。")]
    public string MemberId { get; set; } = string.Empty;

    /// <summary>
    /// 账单分类标识。
    /// </summary>
    [Required(ErrorMessage = "请选择分类。")]
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>
    /// 账单收支类型。
    /// </summary>
    [Required(ErrorMessage = "请选择收支类型。")]
    public TransactionKind Kind { get; set; } = TransactionKind.Expense;

    /// <summary>
    /// 账单金额。
    /// </summary>
    [Range(typeof(decimal), "0.01", "9999999", ErrorMessage = "金额必须大于 0。")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 商户、商品或账单名称。
    /// </summary>
    [Required(ErrorMessage = "请输入账单名称。")]
    [StringLength(60, ErrorMessage = "账单名称不能超过 60 个字符。")]
    public string MerchantName { get; set; } = string.Empty;

    /// <summary>
    /// 支付方式。
    /// </summary>
    [StringLength(40, ErrorMessage = "支付方式不能超过 40 个字符。")]
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// 账单备注。
    /// </summary>
    [StringLength(200, ErrorMessage = "备注不能超过 200 个字符。")]
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// 账单发生时间。
    /// </summary>
    public DateTimeOffset? OccurredAt { get; set; } = DateTimeOffset.Now;
}
