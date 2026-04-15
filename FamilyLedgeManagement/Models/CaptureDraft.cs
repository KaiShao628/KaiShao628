using DatabaseCommon.EntityBase;
using FamilyLedgeManagement.Enums;

namespace FamilyLedgeManagement.Models;

/// <summary>
/// 截图识别记录，保留截图、OCR 结果和识别出的记账字段。
/// </summary>
public sealed class CaptureDraft : Entity
{
    /// <summary>
    /// 归属家庭成员标识。
    /// </summary>
    public string MemberId { get; set; } = string.Empty;

    /// <summary>
    /// 系统建议的账单分类标识。
    /// </summary>
    public string SuggestedCategoryId { get; set; } = string.Empty;

    /// <summary>
    /// OCR 或用户手动补充得到的建议金额。
    /// </summary>
    public decimal? SuggestedAmount { get; set; }

    /// <summary>
    /// OCR 识别出的商户或门店名称。
    /// </summary>
    public string MerchantName { get; set; } = string.Empty;

    /// <summary>
    /// OCR 识别出的商品或订单名称。
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// OCR 识别出的支付方式。
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// 截图来源，例如 iPhone Shortcut 或页面上传。
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// OCR 返回的原始识别文本。
    /// </summary>
    public string RecognizedText { get; set; } = string.Empty;

    /// <summary>
    /// 已保存截图的访问地址。
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// 用户上传时的原始文件名。
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// OCR 从截图中识别出的支付发生时间。
    /// </summary>
    public DateTimeOffset? RecognizedOccurredAt { get; set; }

    /// <summary>
    /// 截图被系统接收的时间。
    /// </summary>
    public DateTimeOffset CapturedAt { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// 截图识别记录处理状态。
    /// </summary>
    public CaptureDraftStatus Status { get; set; } = CaptureDraftStatus.Pending;
}
