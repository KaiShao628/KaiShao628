using System.Globalization;
using System.Text.RegularExpressions;

namespace FamilyLedgeManagement.Services;

/// <summary>
/// 文本规则识别服务，作为没有图片 OCR 时的轻量级兜底实现。
/// </summary>
public sealed class CaptureRecognitionService
{
    /// <summary>
    /// 优先匹配带支付语义前缀的金额。
    /// </summary>
    private static readonly Regex PreferredAmountRegex = new("(?:支付|实付|合计|金额|付款|消费)[^\\d]{0,6}(?<amount>\\d+(?:\\.\\d{1,2})?)", RegexOptions.Compiled);

    /// <summary>
    /// 无明确前缀时兜底匹配普通金额。
    /// </summary>
    private static readonly Regex FallbackAmountRegex = new(@"(?<!\d)(?<amount>\d{1,6}(?:\.\d{1,2})?)(?!\d)", RegexOptions.Compiled);

    /// <summary>
    /// 匹配常见商户字段。
    /// </summary>
    private static readonly Regex MerchantRegex = new("(?:商户|收款方|收款商户|付款给|商家|店铺|门店)[：:\\s]*(?<merchant>[^\\r\\n]+)", RegexOptions.Compiled);

    /// <summary>
    /// 从已有文本中分析金额和商户。
    /// </summary>
    public Task<CaptureRecognitionResult> AnalyzeAsync(
        string imagePath,
        string existingText,
        decimal? existingAmount,
        string existingMerchant,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var mergedText = string.IsNullOrWhiteSpace(existingText) ? string.Empty : existingText.Trim();

        return Task.FromResult(new CaptureRecognitionResult
        {
            RecognizedText = mergedText,
            SuggestedAmount = existingAmount ?? ExtractAmount(mergedText),
            MerchantName = string.IsNullOrWhiteSpace(existingMerchant) ? ExtractMerchant(mergedText) : existingMerchant.Trim()
        });
    }

    /// <summary>
    /// 从文本中提取金额。
    /// </summary>
    private static decimal? ExtractAmount(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var preferred = PreferredAmountRegex.Match(text);
        if (preferred.Success && decimal.TryParse(preferred.Groups["amount"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var preferredAmount))
        {
            return preferredAmount;
        }

        var candidates = FallbackAmountRegex.Matches(text)
            .Select(x => x.Groups["amount"].Value)
            .Where(x => decimal.TryParse(x, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
            .Select(x => decimal.Parse(x, NumberStyles.Number, CultureInfo.InvariantCulture))
            .Where(x => x is > 0 and < 1000000)
            .OrderByDescending(x => x)
            .ToList();

        return candidates.FirstOrDefault();
    }

    /// <summary>
    /// 从文本中提取商户。
    /// </summary>
    private static string ExtractMerchant(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var merchantMatch = MerchantRegex.Match(text);
        if (merchantMatch.Success)
        {
            return SanitizeMerchant(merchantMatch.Groups["merchant"].Value);
        }

        var firstCandidate = text
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(SanitizeMerchant)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x) && x.Length is > 1 and < 30 && !x.Contains("支付成功") && !x.Contains("完成"));

        return firstCandidate ?? string.Empty;
    }

    /// <summary>
    /// 清理商户字段中的标签和标点。
    /// </summary>
    private static string SanitizeMerchant(string value)
        => value.Replace("商户", string.Empty, StringComparison.OrdinalIgnoreCase).Trim('：', ':', '-', ' ');
}

/// <summary>
/// 文本规则识别结果。
/// </summary>
public sealed class CaptureRecognitionResult
{
    /// <summary>
    /// 原始识别文本。
    /// </summary>
    public string RecognizedText { get; set; } = string.Empty;

    /// <summary>
    /// 建议金额。
    /// </summary>
    public decimal? SuggestedAmount { get; set; }

    /// <summary>
    /// 商户或门店名称。
    /// </summary>
    public string MerchantName { get; set; } = string.Empty;

    /// <summary>
    /// 商品或订单名称。
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 支付方式。
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// 账单发生时间。
    /// </summary>
    public DateTimeOffset? OccurredAt { get; set; }
}
