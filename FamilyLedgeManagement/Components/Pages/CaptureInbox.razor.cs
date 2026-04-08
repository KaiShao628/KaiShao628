using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Services;
using Microsoft.AspNetCore.Components;

namespace FamilyLedgeManagement.Components.Pages;

public partial class CaptureInbox
{
    [Inject]
    private LedgerService LedgerService { get; set; } = default!;

    protected IReadOnlyList<CaptureDraftListItemDto> Drafts { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        await ReloadAsync();
    }

    protected async Task PromoteAsync(string draftId)
    {
        await LedgerService.PromoteCaptureDraftAsync(draftId);
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        Drafts = await LedgerService.GetCaptureDraftsAsync();
    }
}

