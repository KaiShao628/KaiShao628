namespace FamilyLedgeManagement.Dtos;

/// <summary>
/// 截图记账上传请求 DTO。
/// </summary>
public sealed class CaptureEntryRequestDto
{
    /// <summary>
    /// 账单归属成员标识。
    /// </summary>
    public string MemberId { get; set; } = string.Empty;

    /// <summary>
    /// 建议分类标识。
    /// </summary>
    public string SuggestedCategoryId { get; set; } = string.Empty;

    /// <summary>
    /// 用户手动输入或 OCR 识别出的金额。
    /// </summary>
    public decimal? SuggestedAmount { get; set; }

    /// <summary>
    /// 用户手动输入或 OCR 识别出的商户名称。
    /// </summary>
    public string MerchantName { get; set; } = string.Empty;

    /// <summary>
    /// 截图来源。
    /// </summary>
    public string Source { get; set; } = "iPhone Shortcut";

    /// <summary>
    /// 已有 OCR 文本，可作为识别补充输入。
    /// </summary>
    public string RecognizedText { get; set; } = string.Empty;

    /// <summary>
    /// 截图发生或上传时间。
    /// </summary>
    public DateTimeOffset? CapturedAt { get; set; }
}
