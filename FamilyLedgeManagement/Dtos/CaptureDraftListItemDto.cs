using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Dtos;

public sealed class CaptureDraftListItemDto
{
    public string Id { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal? SuggestedAmount { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string RecognizedText { get; set; } = string.Empty;
    public DateTimeOffset CapturedAt { get; set; }
    public CaptureDraftStatus Status { get; set; }
}

