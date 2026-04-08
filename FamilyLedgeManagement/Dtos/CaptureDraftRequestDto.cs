namespace FamilyLedgeManagement.Dtos;

public sealed class CaptureDraftRequestDto
{
    public string MemberId { get; set; } = string.Empty;
    public string SuggestedCategoryId { get; set; } = string.Empty;
    public decimal? SuggestedAmount { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string Source { get; set; } = "iPhone Shortcut";
    public string RecognizedText { get; set; } = string.Empty;
    public DateTimeOffset? CapturedAt { get; set; }
}

