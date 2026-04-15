using DatabaseCommon.EntityBase;

namespace FamilyLedgeManagement.Models;

/// <summary>
/// 家庭成员实体。
/// </summary>
public sealed class FamilyMember : Entity
{
    /// <summary>
    /// 成员显示名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 成员角色或备注说明。
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 成员主题色，用于页面标识。
    /// </summary>
    public string AccentColor { get; set; } = "#245b90";
}
