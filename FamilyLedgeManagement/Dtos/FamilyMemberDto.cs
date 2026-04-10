namespace FamilyLedgeManagement.Dtos;

/// <summary>
/// 家庭成员只读展示 DTO。
/// </summary>
public sealed class FamilyMemberDto
{
    /// <summary>
    /// 成员唯一标识。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 成员显示名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 成员角色或备注说明。
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 成员主题色。
    /// </summary>
    public string AccentColor { get; set; } = "#245b90";
}
