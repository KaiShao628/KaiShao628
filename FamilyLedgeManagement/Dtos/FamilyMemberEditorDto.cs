using System.ComponentModel.DataAnnotations;

namespace FamilyLedgeManagement.Dtos;

/// <summary>
/// 家庭成员新增和编辑 DTO。
/// </summary>
public sealed class FamilyMemberEditorDto
{
    /// <summary>
    /// 成员唯一标识，新建时为空。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 成员显示名称。
    /// </summary>
    [Required(ErrorMessage = "请输入成员名称。")]
    [StringLength(20, ErrorMessage = "成员名称不能超过 20 个字符。")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 成员角色或备注说明。
    /// </summary>
    [Required(ErrorMessage = "请输入成员角色。")]
    [StringLength(30, ErrorMessage = "角色说明不能超过 30 个字符。")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 成员主题色，使用 #RRGGBB 格式。
    /// </summary>
    [Required(ErrorMessage = "请输入主题色。")]
    [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "颜色需要使用 #RRGGBB 格式。")]
    public string AccentColor { get; set; } = "#245b90";
}
