namespace FamilyLedger.Models;

public sealed class CaptureDraft
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string MemberId { get; set; } = string.Empty;
    public string SuggestedCategoryId { get; set; } = string.Empty;
    public decimal? SuggestedAmount { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string RecognizedText { get; set; } = string.Empty;
    public DateTimeOffset CapturedAt { get; set; } = DateTimeOffset.Now;
    public CaptureDraftStatus Status { get; set; } = CaptureDraftStatus.Pending;
}
