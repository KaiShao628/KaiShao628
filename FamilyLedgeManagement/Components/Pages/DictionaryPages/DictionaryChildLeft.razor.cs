using BootstrapBlazor.Components;
using FamilyLedgeManagement.Components.Pages.NormalPages;
using FamilyLedgeManagement.Dtos.DictionaryDtos;
using FamilyLedgeManagement.Services;
using FamilyLedgeManagement.Services.DictionaryServices;
using Microsoft.AspNetCore.Components;

namespace FamilyLedgeManagement.Components.Pages.DictionaryPages
{
    public partial class DictionaryChildLeft
    {
        [CascadingParameter(Name = nameof(OnTreeSelectedIdChanged))] public Action<string>? OnTreeSelectedIdChanged { get; set; }
        [CascadingParameter(Name = nameof(OnOpenLoadding))] public Action? OnOpenLoadding { get; set; }
        [CascadingParameter(Name = nameof(OnCloseLoadding))] public Action? OnCloseLoadding { get; set; }

        /// <summary>
        /// 家庭账本业务服务。
        /// </summary>
        [Inject]
        private DictionaryService DictionaryService { get; set; } = default!;

        private EditDictionary? _editDic;
        private CloudWarpper _cloudWarpper;

        private List<TreeViewItem<DictionaryDto>>? TreeViewItems { get; set; }
        private List<TreeViewItem<DictionaryDto>>? TreeViewItemsSource { get; set; }
        private string? _codeVm = "";
        private List<string> _menuBtnAuth;
        private bool _addButton = false;
        protected override async Task OnInitializedAsync()
        {
            TreeViewItems = await GetTreeViewAsync();
            TreeViewItemsSource = new List<TreeViewItem<DictionaryDto>>(TreeViewItems);
        }

        private async Task<List<TreeViewItem<DictionaryDto>>?> GetTreeViewAsync()
        {
            var treeView = await DictionaryService.GetTreeViewItemsAsync();
            if (treeView.Any())
            {
                treeView = treeView.Select(x =>
                {
                    x.Template = y => BootstrapDynamicComponent.CreateComponent<DictionaryChildLeftItem>(new Dictionary<string, object?>()
                    {
                        [nameof(DictionaryChildLeftItem.DictionaryDto)] = y,
                        [nameof(DictionaryChildLeftItem.AfterEdit)] = EventCallback.Factory.Create(this, AfterEdit),
                        [nameof(DictionaryChildLeftItem.EditButton)] = _menuBtnAuth.Contains("DicEdit"),
                        [nameof(DictionaryChildLeftItem.DeleteButton)] = _menuBtnAuth.Contains("DicDelete"),
                    }).Render();
                    return x;
                }).ToList();
            }
            return treeView;
        }

        private async Task OnSearchAsync(string vv = "")
        {
            if (TreeViewItems != null && !string.IsNullOrWhiteSpace(_codeVm))
            {
                TreeViewItems = TreeViewItemsSource.Where(x => x.Value.DictionaryCode.Contains(_codeVm.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

                StateHasChanged();
            }
            else
            {
                TreeViewItems = TreeViewItemsSource;
                StateHasChanged();
            }
            await Task.CompletedTask;
        }

        private Task OnTreeItemClick(TreeViewItem<DictionaryDto> treeView)
        {
            if (treeView.Value.DictionaryCode == "全部")
            {
                OnTreeSelectedIdChanged?.Invoke("");
            }
            else
            {
                OnTreeSelectedIdChanged?.Invoke(treeView.Value.Id);
            }
            return Task.CompletedTask;
        }

        private async Task OnAddAsync()
        {
            await _editDic.OpenModal("", "新增字典");
        }

        private async Task AfterEdit()
        {
            TreeViewItems = await GetTreeViewAsync();
            TreeViewItemsSource = new List<TreeViewItem<DictionaryDto>>(await GetTreeViewAsync());
            _codeVm = "";
            await OnSearchAsync();
            OnTreeSelectedIdChanged?.Invoke("");
        }
    }
}