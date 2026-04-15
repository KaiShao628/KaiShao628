using BootstrapBlazor.Components;
using FamilyLedgeManagement.Dtos.DictionaryDtos;
using FamilyLedgeManagement.Services.DictionaryServices;
using FamilyLedgeManagement.Utilities;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace FamilyLedgeManagement.Components.Pages.DictionaryPages
{
    public partial class DictionaryChildLeftItem
    {
        [Parameter]
        [NotNull]
        public DictionaryDto DictionaryDto { get; set; }
        [Parameter]
        public bool EditButton { get; set; }
        [Parameter]
        public bool DeleteButton { get; set; }

        [Parameter]
        public EventCallback AfterEdit { get; set; }

        [Inject]
        private DictionaryService DictionaryService { get; set; } = default!;
        private EditDictionary? _editDic;
        //private UserInfoVO? _basicPlatformUserInfo;

        protected override async Task OnInitializedAsync()
        {
            //_basicPlatformUserInfo = await LocalService.GetItem<UserInfoVO>(ProjectStorageEnum.ProjectUser);
        }

        private async Task OnEditAsync()
        {
            await _editDic.OpenModal(DictionaryDto.Id, "编辑字典");
        }

        private async Task OnDeleteAsync()
        {
            var op = new SwalOption()
            {
                Category = SwalCategory.Warning,
                Title = "提示",
                Content = "确认删除吗？",
                OnConfirmAsync = async () => await OnDeleteItemAsync(DictionaryDto.Id, " _basicPlatformUserInfo.Id"),
            };

            await SwalService.ShowModal(op);

            await AfterEdit.InvokeAsync();

            StateHasChanged();
        }

        private async Task OnDeleteItemAsync(string id, string userId)
        {
            var result = await DictionaryService.DeleteItemAsync(id, userId);
            if (result.Data)
            {
                await FamilyLedgeMessageHelper.TosatSuccessAsync("删除字典", FamilyLedgeMessageHelper.DeleteSuccessContent);
            }
            else
            {
                string msg = string.IsNullOrWhiteSpace(result.Message) ? FamilyLedgeMessageHelper.DeleteErrorContent : $"删除失败，失败原因：{result.Message}" + FamilyLedgeMessageHelper.ContentTemplate;
                await FamilyLedgeMessageHelper.TosatErrorAsync("删除配件组", msg);
            }
        }

        public async Task AfterEditAsync()
        {
            await AfterEdit.InvokeAsync();
        }
    }
}