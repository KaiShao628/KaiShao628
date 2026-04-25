using FamilyLedgeManagement.Dtos.BaseDtos;
using FamilyLedgeManagement.Enums;

namespace FamilyLedgeManagement.Dtos;

/// <summary>
/// 账单列表展示 DTO。
/// </summary>
public sealed class TransactionListItemDto : EntityBaseDto
{
    /// <summary>
    /// 账单归属成员标识。
    /// </summary>
    public string MemberId { get; set; } = string.Empty;

    /// <summary>
    /// 账单分类标识。
    /// </summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>
    /// 账单归属成员名称。
    /// </summary>
    public string MemberName { get; set; } = string.Empty;

    /// <summary>
    /// 账单分类名称。
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 账单收支类型。
    /// </summary>
    public string Kind { get; set; } = string.Empty;
    public string KindName { get; set; } = string.Empty;

    /// <summary>
    /// 账单金额。
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 商户、商品或账单名称。
    /// </summary>
    public string MerchantName { get; set; } = string.Empty;

    /// <summary>
    /// 支付方式。
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// 账单备注。
    /// </summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// 账单发生时间。
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; }
}
