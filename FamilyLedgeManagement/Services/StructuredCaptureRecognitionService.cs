using System.Globalization;
using System.Text.RegularExpressions;
using PaddleOCRSharp;

namespace FamilyLedgeManagement.Services;

/// <summary>
/// 基于 PaddleOCR 的截图结构化识别服务，负责从支付截图中提取金额、时间、商品和支付方式。
/// </summary>
public sealed class StructuredCaptureRecognitionService
{
    /// <summary>
    /// Paddle OCR 引擎采用延迟单例初始化，避免每次识别都重复加载模型。
    /// </summary>
    private static readonly Lazy<PaddleOCREngine> Engine = new(CreateEngine, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// 支付时间字段标签。
    /// </summary>
    private static readonly string[] TimeLabels = ["支付时间"];

    /// <summary>
    /// 商品字段标签。
    /// </summary>
    private static readonly string[] ProductLabels = ["商品"];

    /// <summary>
    /// 支付方式字段标签。
    /// </summary>
    private static readonly string[] PaymentMethodLabels = ["支付方式"];

    /// <summary>
    /// 商户字段标签。
    /// </summary>
    private static readonly string[] MerchantLabels = ["商户", "商家", "门店", "店铺", "收款方"];

    /// <summary>
    /// 常见支付详情字段边界，用于截断某个字段的识别片段。
    /// </summary>
    private static readonly string[] SectionBoundaries = ["当前状态", "支付时间", "商品", "收单机构", "支付方式", "交易单号", "商户单号", "商家小程序", "账单服务"];

    /// <summary>
    /// 普通金额正则，兼容 OCR 把小数点识别成中文句号或中点的情况。
    /// </summary>
    private static readonly Regex DecimalAmountRegex = new(@"(?<!\d)(?<value>[-—−]?\d{1,6}(?:[.·•。]\d{1,2}))(?!\d)", RegexOptions.Compiled);

    /// <summary>
    /// 支付时间正则。
    /// </summary>
    private static readonly Regex TimeRegex = new(@"(?<value>\d{4}年\d{1,2}月\d{1,2}日\d{1,2}:\d{2}:\d{2})", RegexOptions.Compiled);

    /// <summary>
    /// 商品名称中常见的【门店】结构正则。
    /// </summary>
    private static readonly Regex BracketMerchantRegex = new(@"【(?<value>[^】]+)】", RegexOptions.Compiled);

    /// <summary>
    /// 带支付语义前缀的金额正则，优先级高于普通金额正则。
    /// </summary>
    private static readonly Regex AmountPrefixRegex = new(@"(?:支付成功|支付|实付|合计|金额|付款|消费)[^\d\-—−]{0,10}(?<value>[-—−]?\d{1,6}(?:[.·•。]\d{1,2}))", RegexOptions.Compiled);

    /// <summary>
    /// 识别图片并返回结构化结果。
    /// </summary>
    public Task<StructuredCaptureRecognitionResult> AnalyzeAsync(
        string imagePath,
        string existingText,
        decimal? existingAmount,
        string existingMerchant,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var ocrText = ReadTextFromImage(imagePath);
        var mergedText = MergeText(existingText, ocrText);
        var normalizedText = NormalizeText(mergedText);

        var amount = existingAmount ?? ExtractAmount(normalizedText);
        var occurredAt = ExtractOccurredAt(normalizedText);
        var productName = ExtractSectionValue(normalizedText, ProductLabels, ["收单机构", "支付方式", "交易单号", "商户单号", "商家小程序", "账单服务"]);
        var paymentMethod = ExtractSectionValue(normalizedText, PaymentMethodLabels, ["交易单号", "商户单号", "商家小程序", "账单服务"]);
        var merchantName = string.IsNullOrWhiteSpace(existingMerchant)
            ? ExtractMerchant(normalizedText, productName)
            : existingMerchant.Trim();

        return Task.FromResult(new StructuredCaptureRecognitionResult
        {
            RecognizedText = normalizedText,
            SuggestedAmount = amount,
            MerchantName = merchantName,
            ProductName = productName,
            PaymentMethod = paymentMethod,
            OccurredAt = occurredAt
        });
    }

    /// <summary>
    /// 创建 Paddle OCR 引擎。
    /// </summary>
    private static PaddleOCREngine CreateEngine()
    {
        var parameter = new OCRParameter
        {
            use_angle_cls = true,
            cls = true,
            det = true,
            rec = true,
            enable_mkldnn = true,
            cpu_math_library_num_threads = 4
        };

        return new PaddleOCREngine(OCRModelConfig.Default, parameter);
    }

    /// <summary>
    /// 从图片中读取 OCR 文本，失败时返回空字符串以避免阻断上传流程。
    /// </summary>
    private static string ReadTextFromImage(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            return string.Empty;
        }

        try
        {
            var result = Engine.Value.DetectText(imagePath);
            return result?.Text ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 从文本中提取最可信金额。
    /// </summary>
    private static decimal? ExtractAmount(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var prefixMatch = AmountPrefixRegex.Match(text);
        if (prefixMatch.Success && TryParseAmount(prefixMatch.Groups["value"].Value, out var preferred))
        {
            return preferred;
        }

        var candidates = DecimalAmountRegex.Matches(text)
            .Select(x => x.Groups["value"].Value)
            .Select(x => TryParseAmount(x, out var amount) ? amount : (decimal?)null)
            .Where(x => x is > 0 and < 1000000)
            .Select(x => x!.Value)
            .OrderByDescending(x => x)
            .ToList();

        return candidates.FirstOrDefault();
    }

    /// <summary>
    /// 从文本中提取支付发生时间。
    /// </summary>
    private static DateTimeOffset? ExtractOccurredAt(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var section = ExtractSectionValue(text, TimeLabels, ["商品", "收单机构", "支付方式", "交易单号"]);
        var source = string.IsNullOrWhiteSpace(section) ? text : section;
        var match = TimeRegex.Match(source);

        if (!match.Success)
        {
            return null;
        }

        return DateTimeOffset.TryParseExact(
            match.Groups["value"].Value,
            "yyyy年M月d日H:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal,
            out var occurredAt)
            ? occurredAt
            : null;
    }

    /// <summary>
    /// 从商品区块或商户字段中提取商户名称。
    /// </summary>
    private static string ExtractMerchant(string text, string productName)
    {
        var bracketMatch = BracketMerchantRegex.Match(productName);
        if (bracketMatch.Success)
        {
            return SanitizeValue(bracketMatch.Groups["value"].Value);
        }

        var section = ExtractSectionValue(text, MerchantLabels, ["支付方式", "交易单号", "商户单号", "账单服务"]);
        if (!string.IsNullOrWhiteSpace(section))
        {
            return SanitizeValue(section);
        }

        return SanitizeValue(productName);
    }

    /// <summary>
    /// 基于字段标签和边界标签提取字段值。
    /// </summary>
    private static string ExtractSectionValue(string text, IEnumerable<string> labels, IEnumerable<string> boundaries)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        foreach (var label in labels)
        {
            var index = text.IndexOf(label, StringComparison.Ordinal);
            if (index < 0)
            {
                continue;
            }

            var start = index + label.Length;
            while (start < text.Length && (char.IsWhiteSpace(text[start]) || text[start] is ':' or '：'))
            {
                start++;
            }

            var end = text.Length;
            foreach (var boundary in boundaries)
            {
                var boundaryIndex = text.IndexOf(boundary, start, StringComparison.Ordinal);
                if (boundaryIndex >= 0 && boundaryIndex < end)
                {
                    end = boundaryIndex;
                }
            }

            if (end > start)
            {
                return SanitizeValue(text[start..end]);
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 合并用户传入文本和 OCR 文本。
    /// </summary>
    private static string MergeText(string existingText, string ocrText)
    {
        var parts = new[] { existingText?.Trim(), ocrText?.Trim() }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        return string.Join(Environment.NewLine, parts);
    }

    /// <summary>
    /// 归一化 OCR 文本，修复常见标点和空白问题。
    /// </summary>
    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text
            .Replace('\u3000', ' ')
            .Replace('，', ',')
            .Replace('（', '(')
            .Replace('）', ')')
            .Replace('。', '.')
            .Replace('·', '.')
            .Replace('•', '.')
            .Replace('｜', '|')
            .Replace('—', '-')
            .Replace('−', '-')
            .Replace('－', '-');

        normalized = Regex.Replace(normalized, @"(?<=\d)\s*[.]\s*(?=\d)", ".");
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
        return normalized;
    }

    /// <summary>
    /// 将 OCR 金额文本转换为 decimal。
    /// </summary>
    private static bool TryParseAmount(string rawValue, out decimal amount)
    {
        var normalized = rawValue
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("·", ".", StringComparison.Ordinal)
            .Replace("•", ".", StringComparison.Ordinal)
            .Replace("。", ".", StringComparison.Ordinal)
            .Replace("—", "-", StringComparison.Ordinal)
            .Replace("−", "-", StringComparison.Ordinal)
            .Replace("－", "-", StringComparison.Ordinal);

        if (decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out amount))
        {
            amount = Math.Abs(amount);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 清理字段值里的多余文本和空白。
    /// </summary>
    private static string SanitizeValue(string value)
    {
        var sanitized = value.Trim();
        foreach (var boundary in SectionBoundaries)
        {
            var boundaryIndex = sanitized.IndexOf(boundary, StringComparison.Ordinal);
            if (boundaryIndex > 0)
            {
                sanitized = sanitized[..boundaryIndex];
            }
        }

        sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();
        return sanitized;
    }
}

/// <summary>
/// 截图结构化识别结果。
/// </summary>
public sealed class StructuredCaptureRecognitionResult
{
    /// <summary>
    /// OCR 原始识别文本。
    /// </summary>
    public string RecognizedText { get; set; } = string.Empty;

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
    /// 识别出的账单发生时间。
    /// </summary>
    public DateTimeOffset? OccurredAt { get; set; }
}
