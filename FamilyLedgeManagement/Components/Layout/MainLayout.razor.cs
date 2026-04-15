using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace FamilyLedgeManagement.Components.Layout;

public sealed partial class MainLayout
{
    private bool UseTabSet { get; set; } = true;
    private bool IsFixedHeader { get; set; } = true;
    private bool IsFixedTabHeader { get; set; } = true;
    private bool IsFixedFooter { get; set; } = false;
    private bool IsFullSide { get; set; } = true;
    private bool ShowTabInHeader { get; set; } = true;
    private List<MenuItem>? Menus { get; set; }

    protected override void OnInitialized()
    {
        Menus =
        [
            new() { Text = "概览", Icon = "fa-solid fa-fw fa-house", Url = "/", Match = NavLinkMatch.All },
            new() { Text = "快速记账", Icon = "fa-solid fa-fw fa-pen-to-square", Url = "/quick-entry" },
            new() { Text = "账单流水", Icon = "fa-solid fa-fw fa-table-list", Url = "/transactions" },
            new() { Text = "截图记账", Icon = "fa-solid fa-fw fa-camera-retro", Url = "/capture-inbox" },
            new() { Text = "成员管理", Icon = "fa-solid fa-fw fa-users", Url = "/members" },
            new() { Text = "分类管理", Icon = "fa-solid fa-fw fa-tags", Url = "/categories" },
            new() { Text = "字典管理", Icon = "fa-solid fa-book", Url = "/diclist" }
        ];
    }
}

