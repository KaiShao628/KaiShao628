using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Services;
using Microsoft.AspNetCore.Components;

namespace FamilyLedgeManagement.Components.Pages;

public partial class Transactions
{
    [Inject]
    private LedgerService LedgerService { get; set; } = default!;

    protected string SelectedMonthText { get; set; } = $"{DateTime.Today:yyyy-MM}";
    protected List<string> MonthOptions { get; set; } = [];
    protected IReadOnlyList<TransactionListItemDto> Items { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        MonthOptions = Enumerable.Range(0, 12).Select(offset => DateTime.Today.AddMonths(-offset).ToString("yyyy-MM")).ToList();
        await ReloadAsync();
    }

    protected async Task ReloadAsync()
    {
        var month = DateOnly.Parse($"{SelectedMonthText}-01");
        Items = await LedgerService.GetTransactionsAsync(month);
    }
}

