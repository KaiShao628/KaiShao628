namespace FamilyLedgeManagement.Dtos;

/// <summary>
/// 快速记账页面基础上下文 DTO。
/// </summary>
public sealed class QuickEntryContextDto
{
    /// <summary>
    /// 可选家庭成员列表。
    /// </summary>
    public IReadOnlyList<FamilyMemberDto> Members { get; set; } = [];

    /// <summary>
    /// 可选账单分类列表。
    /// </summary>
    public IReadOnlyList<LedgerCategoryDto> Categories { get; set; } = [];

    /// <summary>
    /// 常用支付方式列表。
    /// </summary>
    public IReadOnlyList<string> PaymentMethods { get; set; } = [];
}
