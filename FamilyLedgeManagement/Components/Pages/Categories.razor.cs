using BootstrapBlazor.Components;
using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Enums;
using FamilyLedgeManagement.Services;
using Microsoft.AspNetCore.Components;

namespace FamilyLedgeManagement.Components.Pages;

public partial class Categories
{
    /// <summary>
    /// 家庭账本业务服务。
    /// </summary>
    [Inject]
    private LedgerService LedgerService { get; set; } = default!;

    /// <summary>
    /// 分类表格数据源。
    /// </summary>
    protected List<LedgerCategoryEditorDto> Items { get; set; } = [];

    /// <summary>
    /// 当前表格选中的分类行。
    /// </summary>
    protected List<LedgerCategoryEditorDto> SelectedRows { get; set; } = [];

    /// <summary>
    /// 表格每页条数选项，默认使用 59 条/页。
    /// </summary>
    protected IEnumerable<int> DefaultPageItemsSource { get; } = [59, 100, 200];

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
        await ReloadAsync();
    }

    protected Task OnSelectedRowsChanged(IEnumerable<LedgerCategoryEditorDto> rows)
    {
        SelectedRows = rows.ToList();
        return Task.CompletedTask;
    }

    protected Task<LedgerCategoryEditorDto> OnAddAsync()
    {
        ResetStatus();
        return Task.FromResult(CreateNewEditor());
    }

    protected async Task<bool> OnSaveAsync(LedgerCategoryEditorDto item, ItemChangedType _)
    {
        ResetStatus();

        try
        {
            await LedgerService.SaveCategoryAsync(item);
            Message = $"已保存分类：{item.Name}";
            await ReloadAsync();
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
    }

    protected async Task<bool> OnDeleteAsync(IEnumerable<LedgerCategoryEditorDto> items)
    {
        ResetStatus();
        var rows = items.ToList();

        try
        {
            foreach (var row in rows)
            {
                await LedgerService.DeleteCategoryAsync(row.Id);
            }

            Message = $"已删除 {rows.Count} 个分类";
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
        Items = (await LedgerService.GetCategoriesAsync())
            .Select(x => new LedgerCategoryEditorDto
            {
                Id = x.Id,
                Name = x.Name,
                Kind = "",
                Color = x.Color
            })
            .ToList();
    }

    private void ResetStatus()
    {
        Message = null;
        ErrorMessage = null;
    }

    private static LedgerCategoryEditorDto CreateNewEditor() => new()
    {
        Kind = "",
        Color = "#dc6e2f"
    };
}



