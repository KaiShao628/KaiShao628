namespace FamilyLedgeManagement.Dtos;

/// <summary>
/// 截图 OCR 预识别结果 DTO。
/// </summary>
public sealed class CaptureRecognitionPreviewDto
{
    /// <summary>
    /// 建议金额。
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
    /// OCR 原始识别文本。
    /// </summary>
    public string RecognizedText { get; set; } = string.Empty;

    /// <summary>
    /// 识别出的账单发生时间。
    /// </summary>
    public DateTimeOffset? OccurredAt { get; set; }
}
