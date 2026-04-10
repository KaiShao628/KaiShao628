using BootstrapBlazor.Components;
using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Services;
using Microsoft.AspNetCore.Components;

namespace FamilyLedgeManagement.Components.Pages;

public partial class Members
{
    /// <summary>
    /// 家庭账本业务服务。
    /// </summary>
    [Inject]
    private LedgerService LedgerService { get; set; } = default!;

    /// <summary>
    /// 成员表格数据源。
    /// </summary>
    protected List<FamilyMemberEditorDto> Items { get; set; } = [];

    /// <summary>
    /// 当前表格选中的成员行。
    /// </summary>
    protected List<FamilyMemberEditorDto> SelectedRows { get; set; } = [];

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

    protected Task OnSelectedRowsChanged(IEnumerable<FamilyMemberEditorDto> rows)
    {
        SelectedRows = rows.ToList();
        return Task.CompletedTask;
    }

    protected Task<FamilyMemberEditorDto> OnAddAsync()
    {
        ResetStatus();
        return Task.FromResult(CreateNewEditor());
    }

    protected async Task<bool> OnSaveAsync(FamilyMemberEditorDto item, ItemChangedType _)
    {
        ResetStatus();

        try
        {
            await LedgerService.SaveMemberAsync(item);
            Message = $"已保存成员：{item.Name}";
            await ReloadAsync();
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
    }

    protected async Task<bool> OnDeleteAsync(IEnumerable<FamilyMemberEditorDto> items)
    {
        ResetStatus();
        var rows = items.ToList();

        try
        {
            foreach (var row in rows)
            {
                await LedgerService.DeleteMemberAsync(row.Id);
            }

            Message = $"已删除 {rows.Count} 个成员";
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
        Items = (await LedgerService.GetMembersAsync())
            .Select(x => new FamilyMemberEditorDto
            {
                Id = x.Id,
                Name = x.Name,
                Role = x.Role,
                AccentColor = x.AccentColor
            })
            .ToList();
    }

    private void ResetStatus()
    {
        Message = null;
        ErrorMessage = null;
    }

    private static FamilyMemberEditorDto CreateNewEditor() => new()
    {
        AccentColor = "#245b90"
    };
}



