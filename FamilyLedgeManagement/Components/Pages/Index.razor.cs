using BootstrapBlazor.Components;
using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Models;
using FamilyLedgeManagement.Services;
using Microsoft.AspNetCore.Components;

namespace FamilyLedgeManagement.Components.Pages;

public partial class Index
{
    /// <summary>
    /// 家庭账本业务服务。
    /// </summary>
    [Inject]
    private LedgerService LedgerService { get; set; } = default!;

    /// <summary>
    /// 首页概览数据。
    /// </summary>
    protected DashboardSnapshotDto Dashboard { get; set; } = new();

    /// <summary>
    /// 快速记账上下文，用于最近账单弹窗编辑时展示下拉选项。
    /// </summary>
    protected QuickEntryContextDto? Context { get; set; }

    /// <summary>
    /// 最近账单表格数据源。
    /// </summary>
    protected List<TransactionEditorDto> RecentItems { get; set; } = [];

    /// <summary>
    /// 当前表格选中的最近账单行。
    /// </summary>
    protected List<TransactionEditorDto> SelectedRows { get; set; } = [];

    /// <summary>
    /// 表格每页条数选项，默认使用 59 条/页。
    /// </summary>
    protected IEnumerable<int> DefaultPageItemsSource { get; } = [59, 100, 200];

    /// <summary>
    /// 成员字段的下拉映射数据。
    /// </summary>
    protected IEnumerable<SelectedItem> MemberLookup => Context?.Members.Select(x => new SelectedItem(x.Id, x.Name)) ?? [];

    /// <summary>
    /// 分类字段的下拉映射数据。
    /// </summary>
    protected IEnumerable<SelectedItem> CategoryLookup => Context?.Categories.Select(x => new SelectedItem(x.Id, x.Name)) ?? [];

    /// <summary>
    /// 支付方式字段的下拉映射数据。
    /// </summary>
    protected IEnumerable<SelectedItem> PaymentMethodLookup => Context?.PaymentMethods.Select(x => new SelectedItem(x, x)) ?? [];

    /// <summary>
    /// 页面成功提示。
    /// </summary>
    protected string? Message { get; set; }

    /// <summary>
    /// 页面错误提示。
    /// </summary>
    protected string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Context = await LedgerService.GetQuickEntryContextAsync();
        await ReloadAsync();
    }

    protected Task OnSelectedRowsChanged(IEnumerable<TransactionEditorDto> rows)
    {
        SelectedRows = rows.ToList();
        return Task.CompletedTask;
    }

    protected Task<TransactionEditorDto> OnAddAsync()
    {
        ResetStatus();
        return Task.FromResult(CreateNewEditor());
    }

    protected async Task<bool> OnSaveAsync(TransactionEditorDto item, ItemChangedType _)
    {
        ResetStatus();

        try
        {
            await LedgerService.SaveTransactionAsync(item);
            Message = string.IsNullOrWhiteSpace(item.Id)
                ? $"已新增账单：{item.MerchantName}"
                : $"已更新账单：{item.MerchantName}";
            await ReloadAsync();
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
    }

    protected async Task<bool> OnDeleteAsync(IEnumerable<TransactionEditorDto> items)
    {
        ResetStatus();
        var rows = items.ToList();

        try
        {
            foreach (var row in rows)
            {
                await LedgerService.DeleteTransactionAsync(row.Id);
            }

            Message = $"已删除 {rows.Count} 条账单";
            SelectedRows.Clear();
            await ReloadAsync();
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
    }

    private async Task ReloadAsync()
    {
        Dashboard = await LedgerService.GetDashboardAsync();
        RecentItems = Dashboard.RecentTransactions.Select(MapEditor).ToList();
    }

    private TransactionEditorDto CreateNewEditor()
    {
        var memberId = Context?.Members.FirstOrDefault()?.Id ?? string.Empty;
        var categoryId = Context?.Categories.FirstOrDefault(x => x.Kind == TransactionKind.Expense)?.Id ?? string.Empty;
        var paymentMethod = Context?.PaymentMethods.FirstOrDefault() ?? "微信支付";

        return new TransactionEditorDto
        {
            MemberId = memberId,
            CategoryId = categoryId,
            Kind = TransactionKind.Expense,
            PaymentMethod = paymentMethod,
            OccurredAt = DateTimeOffset.Now
        };
    }

    private static TransactionEditorDto MapEditor(TransactionListItemDto item) => new()
    {
        Id = item.Id,
        MemberId = item.MemberId,
        CategoryId = item.CategoryId,
        Kind = item.Kind,
        Amount = item.Amount,
        MerchantName = item.MerchantName,
        PaymentMethod = item.PaymentMethod,
        Note = item.Note == "-" ? string.Empty : item.Note,
        OccurredAt = item.OccurredAt
    };

    private void ResetStatus()
    {
        Message = null;
        ErrorMessage = null;
    }
}

