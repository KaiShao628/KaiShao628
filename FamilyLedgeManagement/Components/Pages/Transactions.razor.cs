using BootstrapBlazor.Components;
using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Dtos.BaseDtos;
using FamilyLedgeManagement.Dtos.DictionaryDtos;
using FamilyLedgeManagement.Enums;
using FamilyLedgeManagement.Services;
using Microsoft.AspNetCore.Components;

namespace FamilyLedgeManagement.Components.Pages;

public partial class Transactions
{
    /// <summary>
    /// 家庭账本聚合业务服务。
    /// </summary>
    [Inject]
    private TransactionService TransactionService { get; set; } = default!;
    /// <summary>
    /// 家庭账本聚合业务服务。
    /// </summary>
    [Inject]
    private LedgerService LedgerService { get; set; } = default!;

    /// <summary>
    /// 快速记账页面所需的成员、分类和支付方式上下文。
    /// </summary>
    private QuickEntryContextDto? Context { get; set; }

    /// <summary>
    /// 当前账单列表筛选月份，格式为 yyyy-MM。
    /// </summary>
    private string SelectedMonthText { get; set; } = $"{DateTime.Today:yyyy-MM}";

    /// <summary>
    /// 月份筛选下拉选项。
    /// </summary>
    private List<SelectedItem> SelectMonthItems { get; set; } = [];

    /// <summary>
    /// 当前表格选中的账单行。
    /// </summary>
    private List<TransactionListItemDto> SelectedRows { get; set; } = [];

    /// <summary>
    /// 成员字段的下拉映射数据。
    /// </summary>
    private IEnumerable<SelectedItem> MemberLookup => Context?.Members.Select(x => new SelectedItem(x.Id, x.Name)) ?? [];

    /// <summary>
    /// 分类字段的下拉映射数据。
    /// </summary>
    private IEnumerable<SelectedItem> CategoryLookup => Context?.Categories.Select(x => new SelectedItem(x.Id, x.Name)) ?? [];

    /// <summary>
    /// 支付方式字段的下拉映射数据。
    /// </summary>
    private IEnumerable<SelectedItem> PaymentMethodLookup => Context?.PaymentMethods.Select(x => new SelectedItem(x, x)) ?? [];

    /// <summary>
    /// 页面成功提示。
    /// </summary>
    private string? Message { get; set; }

    /// <summary>
    /// 页面错误提示。
    /// </summary>
    private string? ErrorMessage { get; set; }

    private Table<TransactionListItemDto>? _transactionTable;

    protected override async Task OnInitializedAsync()
    {
        Context = await LedgerService.GetQuickEntryContextAsync();
        SelectMonthItems = Enumerable.Range(0, 12)
            .Select(offset => DateTime.Today.AddMonths(-offset).ToString("yyyy-MM"))
            .Select(x => new SelectedItem(x, x))
            .ToList();
    }

    private async Task<QueryData<TransactionListItemDto>> OnQueryAsync(QueryPageOptions options)
    {
        var stratDate = DateTimeOffset.Parse($"{SelectedMonthText}-01");
        var endDate = stratDate.AddMonths(1).AddDays(-1);
        var total = 0;
        var resultItems = new List<TransactionListItemDto>();
        var filter = new FilterDto()
        {
            PageIndex = options.PageIndex,
            PageSize = options.PageItems,
            SearchText = options.SearchText,
            TransactionStratDate = stratDate,
            TransactionEndDate = endDate,
        };
        var tableResult = await TransactionService.OnQueryAsync(filter);
        if (tableResult.Data != null && tableResult.Data.Result.Count > 0)
        {
            resultItems = tableResult.Data.Result;
            total = tableResult.Data.TotalPages;
        }
        return new QueryData<TransactionListItemDto>()
        {
            Items = resultItems,

            TotalCount = total,

            IsFiltered = true,

            IsSearch = true,

            IsAdvanceSearch = true,

            IsSorted = true,
        };

    }

    private Task<TransactionListItemDto> OnAddAsync()
    {
        ResetStatus();
        return Task.FromResult(CreateNewEditor());
    }

    private async Task<bool> OnSaveAsync(TransactionListItemDto item, ItemChangedType _)
    {
        ResetStatus();

        try
        {
            var isCreate = string.IsNullOrWhiteSpace(item.Id);
            await LedgerService.SaveTransactionAsync(item);
            Message = isCreate
                ? $"已新增账单：{item.MerchantName}"
                : $"已更新账单：{item.MerchantName}";
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
    }

    private async Task<bool> OnDeleteAsync(IEnumerable<TransactionListItemDto> items)
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
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
    }

    private Task OnResetSearchAsync(TransactionListItemDto _)
    {
        SelectedMonthText = $"{DateTime.Today:yyyy-MM}";
        ResetStatus();
        return Task.CompletedTask;
    }

    private TransactionListItemDto CreateNewEditor()
    {
        var memberId = Context?.Members.FirstOrDefault()?.Id ?? string.Empty;
        var categoryId = Context?.Categories.FirstOrDefault(x => x.Kind == "Expense")?.Id ?? string.Empty;
        var paymentMethod = Context?.PaymentMethods.FirstOrDefault() ?? "微信支付";

        return new TransactionListItemDto
        {
            MemberId = memberId,
            CategoryId = categoryId,
            Kind = "",
            PaymentMethod = paymentMethod,
            OccurredAt = DateTimeOffset.Now
        };
    }

    private void ResetStatus()
    {
        Message = null;
        ErrorMessage = null;
    }
}
