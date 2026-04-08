using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Services;
using Microsoft.AspNetCore.Components;

namespace FamilyLedgeManagement.Components.Pages;

public partial class Index
{
    [Inject]
    private LedgerService LedgerService { get; set; } = default!;

    protected DashboardSnapshotDto Dashboard { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Dashboard = await LedgerService.GetDashboardAsync();
    }
}

