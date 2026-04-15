using System.ComponentModel.DataAnnotations;
using FamilyLedgeManagement.Enums;

namespace FamilyLedgeManagement.Dtos;

/// <summary>
/// 快速新增账单请求 DTO。
/// </summary>
public sealed class QuickEntryRequestDto
{
    /// <summary>
    /// 账单收支类型。
    /// </summary>
    public TransactionKind Kind { get; set; } = TransactionKind.Expense;

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
    /// 账单金额。
    /// </summary>
    [Range(typeof(decimal), "0.01", "9999999", ErrorMessage = "金额必须大于 0。")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 商户、商品或账单名称。
    /// </summary>
    public string MerchantName { get; set; } = string.Empty;

    /// <summary>
    /// 支付方式。
    /// </summary>
    public string PaymentMethod { get; set; } = "微信支付";

    /// <summary>
    /// 账单备注。
    /// </summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// 账单发生时间，为空时使用当前时间。
    /// </summary>
    public DateTimeOffset? OccurredAt { get; set; }
}
