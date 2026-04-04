namespace FamilyLedger.Models;

public sealed class CaptureDraftListItem
{
    public string Id { get; init; } = string.Empty;
    public string MemberName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public decimal? SuggestedAmount { get; init; }
    public string MerchantName { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string RecognizedText { get; init; } = string.Empty;
    public DateTimeOffset CapturedAt { get; init; }
    public CaptureDraftStatus Status { get; init; }
}
