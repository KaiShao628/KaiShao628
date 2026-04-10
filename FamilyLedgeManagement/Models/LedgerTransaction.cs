namespace FamilyLedgeManagement.Models;

/// <summary>
/// 家庭账单实体。
/// </summary>
public sealed class LedgerTransaction
{
    /// <summary>
    /// 账单唯一标识。
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 账单所属成员标识。
    /// </summary>
    public string MemberId { get; set; } = string.Empty;

    /// <summary>
    /// 账单所属分类标识。
    /// </summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>
    /// 账单收支类型。
    /// </summary>
    public TransactionKind Kind { get; set; }

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
    /// 消费或收入实际发生时间。
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// 记录创建时间。
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
}
