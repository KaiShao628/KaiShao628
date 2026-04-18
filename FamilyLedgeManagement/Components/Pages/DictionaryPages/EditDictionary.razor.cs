using BootstrapBlazor.Components;
using FamilyLedgeManagement.Dtos.DictionaryDtos;
using FamilyLedgeManagement.Services.DictionaryServices;
using FamilyLedgeManagement.Utilities;
using Microsoft.AspNetCore.Components;

namespace FamilyLedgeManagement.Components.Pages.DictionaryPages
{
    public partial class EditDictionary
    {
        [Parameter]
        public EventCallback AfterEdit { get; set; }

        [Inject]
        private DictionaryService DictionaryService { get; set; } = default!;

        [Inject]
        private DictionaryValueService DictionaryValueService { get; set; } = default!;

        private Modal? _modal;
        public string? Title { get; set; }

        public string? _id = "";

        //private UserInfoVO? _basicPlatformUserInfo;
        private Dictionary<string, string>? _languageResDic;
        public string? _dictionaryCode = "字典Code";
        public string? _dictionaryName = "字典名称";
        public bool _isAdd = false;
        private DictionaryDto DictionaryDto { get; set; } = new DictionaryDto();

        //protected override async Task OnInitializedAsync()
        //{
        //    _basicPlatformUserInfo = await LocalService.GetItem<UserInfoVO>(ProjectStorageEnum.ProjectUser);
        //    _languageResDic = await LocalService.GetItem<Dictionary<string, string>>(ProjectStorageEnum.ProjectLanguage);
        //    _dictionaryCode = Helpers.GetLanguage(_languageResDic, "DictionaryCode");
        //    _dictionaryName = Helpers.GetLanguage(_languageResDic, "DictionaryName");
        //}

        public async Task<bool> OnSubmitAsync()
        {
            if (string.IsNullOrWhiteSpace(_id))
            {
                DictionaryDto.CreatorId = " _basicPlatformUserInfo.Id";
                DictionaryDto.CreateTime = DateTime.UtcNow;
                var result = await DictionaryService.AddItemAsync(DictionaryDto);
                if (result.Data)
                {
                    await FamilyLedgeMessageHelper.TosatSuccessAsync("新增字典", FamilyLedgeMessageHelper.SaveSuccessContent);
                    await AfterEdit.InvokeAsync();
                    return true;
                }
                else
                {
                    string errMsg = string.IsNullOrWhiteSpace(result.Message) ? "新增字典失败" + FamilyLedgeMessageHelper.ContentTemplate : $"新增字典失败,错误原因：{result.Message}" + FamilyLedgeMessageHelper.ContentTemplate;
                    await FamilyLedgeMessageHelper.TosatErrorAsync("新增字典", errMsg);
                    return false;
                }
            }
            else
            {
                DictionaryDto.UpdaterId = "_basicPlatformUserInfo.Id";
                var result = await DictionaryService.UpdateItemAsync(DictionaryDto);
                if (result.Data)
                {
                    await FamilyLedgeMessageHelper.TosatSuccessAsync("更新字典", FamilyLedgeMessageHelper.SaveSuccessContent);
                    await AfterEdit.InvokeAsync();
                    return true;
                }
                else
                {
                    string errMsg = string.IsNullOrWhiteSpace(result.Message) ? "更新字典失败" + FamilyLedgeMessageHelper.ContentTemplate : $"更新字典失败,错误原因：{result.Message}" + FamilyLedgeMessageHelper.ContentTemplate;
                    await FamilyLedgeMessageHelper.TosatErrorAsync("更新字典", errMsg);
                    return false;
                }
            }

        }

        public async Task OpenModal(string id, string title)
        {
            Title = title;
            _id = id;
            if (!string.IsNullOrWhiteSpace(_id))
            {
                var result = await DictionaryService.GetEntityAsync(_id);
                if (result.Data != null)
                {
                    DictionaryDto = result.Data;
                }
            }
            else
            {
                DictionaryDto = new DictionaryDto();
            }
            StateHasChanged();
            await _modal.Show();
        }
    }
}