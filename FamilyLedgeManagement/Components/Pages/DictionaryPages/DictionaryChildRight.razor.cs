using BootstrapBlazor.Components;
using FamilyLedgeManagement.Dtos.BaseDtos;
using FamilyLedgeManagement.Dtos.DictionaryDtos;
using FamilyLedgeManagement.Services.DictionaryServices;
using FamilyLedgeManagement.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace FamilyLedgeManagement.Components.Pages.DictionaryPages
{
    public partial class DictionaryChildRight
    {
        [Inject]
        private DictionaryService DictionaryService { get; set; } = default!;

        [Inject]
        private DictionaryValueService DictionaryValueService { get; set; } = default!;

        //[Inject]
        //private IStringLocalizer<DictionaryChildRight> Localizer { get; set; }

        private Table<DictionaryValueDto>? _dicValueTable;

        private string? _dicId;

        //private UserInfoVO? _basicPlatformUserInfo;
        private Dictionary<string, string>? _languageResDic;
        private string? _dicValueSearch;
        private string? _value;
        private string? _cValue;
        private string? _isUsed;
        private string? _updateTime;
        private string? _belongDic;
        private string? _dicValueCode;

        private List<string> _menuBtnAuth;
        private bool _addButton = true;
        private bool _editButton = true;
        private bool _deleteButton = true;
        private bool _showExtendButton = true;

        private List<SelectedItem> SelectDicItems { get; set; } = new List<SelectedItem>();

        protected override async Task OnInitializedAsync()
        {
            //_basicPlatformUserInfo = await LocalService.GetItem<UserInfoVO>(ProjectStorageEnum.ProjectUser);
            //_menuBtnAuth = await LocalService.GetItem<List<string>>(ProjectStorageEnum.ProjectMenuBtnAuth);
            //_languageResDic = await LocalService.GetItem<Dictionary<string, string>>(ProjectStorageEnum.ProjectLanguage);
            _dicValueSearch = Localizer["DicValueSearch"];
            _value = Localizer["Value"];
            _cValue = Localizer["CValue"];
            _isUsed = Localizer["IsUsed"];
            _updateTime = Localizer["UpdateTime"];
            _belongDic = Localizer["BelongDic"];
            _dicValueCode = Localizer["DicValueCode"];

            SelectDicItems = await DictionaryService.GetAllSelectedWithoutAllAsync();

            //await SetBtnAuth();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("resizeTableByClassByTimes", 10);
            }
        }

        private async Task<QueryData<DictionaryValueDto>> OnQueryAsync(QueryPageOptions options)
        {
            var filter = new FilterDto()
            {
                PageIndex = options.PageIndex,
                PageSize = options.PageItems,
                SearchText = options.SearchText,
            };

            if (!string.IsNullOrWhiteSpace(_dicId))
            {
                filter.DicId = _dicId;
            }

            var total = 0;
            var resultItems = new List<DictionaryValueDto>();

            var queryResult = await DictionaryValueService.OnQueryAsync(filter);
            if (queryResult.Data != null && queryResult.Data.Result.Count > 0)
            {
                resultItems = queryResult.Data.Result;
                total = queryResult.Data.TotalPages;
            }
            return new QueryData<DictionaryValueDto>()
            {
                Items = resultItems,

                TotalCount = total,

                IsFiltered = true,

                IsSearch = true,

                IsAdvanceSearch = true,

                IsSorted = true,
            };
        }

        private Task<DictionaryValueDto> OnAddAsync()
        {
            return Task.FromResult(new DictionaryValueDto() { DictionaryId = _dicId ?? "" });
        }

        private async Task<bool> OnSaveAsync(DictionaryValueDto dto, ItemChangedType itemChangedType)
        {
            if (itemChangedType == ItemChangedType.Add)
            {
                //dto.CreatorId = _basicPlatformUserInfo.Id;
                dto.CreateTime = DateTime.UtcNow;
                dto.UpdateTime = DateTime.UtcNow;
                var result = await DictionaryValueService.OnAddAsync(dto);
                if (result.Data)
                {
                    await FamilyLedgeMessageHelper.TosatSuccessAsync("新增字典项", FamilyLedgeMessageHelper.SaveSuccessContent);
                    return true;
                }
                else
                {
                    string errMsg = string.IsNullOrWhiteSpace(result.Message) ? "新增字典项失败" + FamilyLedgeMessageHelper.ContentTemplate : $"新增字典项失败,错误原因：{result.Message}" + FamilyLedgeMessageHelper.ContentTemplate;
                    await FamilyLedgeMessageHelper.TosatErrorAsync("新增字典项", errMsg);
                    return false;
                }
            }
            else
            {
                //dto.UpdaterId = _basicPlatformUserInfo.Id;
                var result = await DictionaryValueService.OnEditAsync(dto);
                if (result.Data)
                {
                    await FamilyLedgeMessageHelper.TosatSuccessAsync("更新字典项", FamilyLedgeMessageHelper.SaveSuccessContent);
                    return true;
                }
                else
                {
                    string errMsg = string.IsNullOrWhiteSpace(result.Message) ? "更新字典项失败" + FamilyLedgeMessageHelper.ContentTemplate : $"更新字典项失败,错误原因：{result.Message}" + FamilyLedgeMessageHelper.ContentTemplate;
                    await FamilyLedgeMessageHelper.TosatErrorAsync("更新字典项", errMsg);
                    return false;
                }
            }
        }

        private async Task<bool> OnDeleteAsync(IEnumerable<DictionaryValueDto> dtos)
        {
            var result = await DictionaryValueService.OnDeleteItemsAsync(dtos.Select(x => x.Id), "_basicPlatformUserInfo.Id");
            if (result.Data)
            {
                await FamilyLedgeMessageHelper.TosatSuccessAsync("删除字典项", FamilyLedgeMessageHelper.DeleteSuccessContent);
                return true;
            }
            else
            {
                string msg = string.IsNullOrWhiteSpace(result.Message) ? "删除归集项目失败" + FamilyLedgeMessageHelper.ContentTemplate : $"删除字典项失败，失败原因：{result.Message}" + FamilyLedgeMessageHelper.ContentTemplate;
                await FamilyLedgeMessageHelper.TosatErrorAsync("删除字典项", msg);
                return false;
            }
        }


        public async Task OnClickSearchAsync(string id)
        {
            _dicId = id;

            await _dicValueTable.QueryAsync();
        }

        public async Task ResetSelectItemsAsync()
        {
            SelectDicItems.Clear();
            SelectDicItems = await DictionaryService.GetAllSelectedWithoutAllAsync();
            StateHasChanged();
        }

        private async Task SetBtnAuth()
        {
            if (_menuBtnAuth.Any())
            {
                _addButton = _menuBtnAuth.Contains("DicValueCreate");
                _editButton = _menuBtnAuth.Contains("DicValueEdit");
                _deleteButton = _menuBtnAuth.Contains("DicValueDelete");

                if (!_editButton && !_deleteButton)
                {
                    _showExtendButton = false;
                }
            }
        }

        private bool SetExtendButton(DictionaryValueDto dto, string key)
        {
            if (key == "edit")
            {
                return _editButton && !dto.IsSystem;
            }
            else
            {
                return _deleteButton && !dto.IsSystem;
            }
        }
    }
}