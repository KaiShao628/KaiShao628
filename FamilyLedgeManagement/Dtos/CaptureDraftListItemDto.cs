using FamilyLedgeManagement.Enums;

namespace FamilyLedgeManagement.Dtos;

/// <summary>
/// 截图识别记录列表展示 DTO。
/// </summary>
public sealed class CaptureDraftListItemDto
{
    /// <summary>
    /// 截图记录唯一标识。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 归属成员名称。
    /// </summary>
    public string MemberName { get; set; } = string.Empty;

    /// <summary>
    /// 建议分类名称。
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 建议账单金额。
    /// </summary>
    public decimal? SuggestedAmount { get; set; }

    /// <summary>
    /// 识别出的商户或门店名称。
    /// </summary>
    public string MerchantName { get; set; } = string.Empty;

    /// <summary>
    /// 识别出的商品或订单名称。
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 识别出的支付方式。
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// 截图来源。
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// OCR 原始识别文本。
    /// </summary>
    public string RecognizedText { get; set; } = string.Empty;

    /// <summary>
    /// 截图访问地址。
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// 原始上传文件名。
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// OCR 识别出的支付发生时间。
    /// </summary>
    public DateTimeOffset? RecognizedOccurredAt { get; set; }

    /// <summary>
    /// 截图接收时间。
    /// </summary>
    public DateTimeOffset CapturedAt { get; set; }

    /// <summary>
    /// 截图记录状态。
    /// </summary>
    public CaptureDraftStatus Status { get; set; }
}
