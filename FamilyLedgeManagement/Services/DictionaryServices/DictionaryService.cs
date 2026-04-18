using BootstrapBlazor.Components;
using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Dtos.DictionaryDtos;
using FamilyLedgeManagement.Enums;
using FamilyLedgeManagement.IRepositories.IDictionaryRepositories;
using FamilyLedgeManagement.Models;
using FamilyLedgeManagement.Models.DictionaryModels;
using FamilyLedgeManagement.Utilities;
using System.Collections;
using System.ComponentModel.Design;

namespace FamilyLedgeManagement.Services.DictionaryServices
{
    internal class DictionaryService : ServiceBase<IDictionaryRepository>
    {
        #region Get Methods
        public async Task<ReturnResult<List<DictionaryDto>>> GetAllItemsListAsync()
        {
            try
            {
                var entityList = await Repository.GetEntityListAsync();

                var dtoList = FamilyLedgeMapper.MapList<DictionaryEntity, DictionaryDto>(entityList);

                return new ResponseResult<List<DictionaryDto>>(DbOpStatus.Success, dtoList);
            }
            catch (Exception ex)
            {
                return new ResponseResult<List<DictionaryDto>>(DbOpStatus.QueryError, ex.Message, null);
            }

        }

        public async Task<ReturnResult<List<DictionaryDto>>> GetAllItemsListWithoutAllAsync()
        {
            try
            {
                var entityList = await Repository.GetEntityListByExpressionAsync(x => !x.IsDeleted && x.DictionaryCode != "全部");

                var dtoList = FamilyLedgeMapper.MapList<DictionaryEntity, DictionaryDto>(entityList);

                return new ResponseResult<List<DictionaryDto>>(DbOpStatus.Success, dtoList);
            }
            catch (Exception ex)
            {
                return new ResponseResult<List<DictionaryDto>>(DbOpStatus.QueryError, ex.Message, null);
            }

        }

        public async Task<ReturnResult<DictionaryDto>> GetEntityAsync(string id)
        {
            try
            {
                var entity = await Repository.GetEntityAsync(id);

                if (entity != null)
                {
                    var dto = FamilyLedgeMapper.Map<DictionaryEntity, DictionaryDto>(entity);

                    return new ResponseResult<DictionaryDto>(DbOpStatus.Success, dto);
                }

                return new ResponseResult<DictionaryDto>(DbOpStatus.QueryError, "获取失败", null);
            }
            catch (Exception ex)
            {
                return new ResponseResult<DictionaryDto>(DbOpStatus.QueryError, ex.Message, null);
            }
        }
        #endregion

        #region Add Methods
        public async Task<ReturnResult<bool>> AddItemAsync(DictionaryDto dictionaryDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dictionaryDto.DictionaryCode))
                {
                    return new ResponseResult<bool>(DbOpStatus.CreateError, "字典Code不可为空", false);
                }

                if (string.IsNullOrWhiteSpace(dictionaryDto.DictionaryName))
                {
                    return new ResponseResult<bool>(DbOpStatus.CreateError, "字典Code重复", false);
                }

                var allDicEntitys = await Repository.GetEntityListAsync();

                var codeAndNameDic = allDicEntitys.ToDictionary(x => x.DictionaryCode, x => x.DictionaryName);

                if (codeAndNameDic.ContainsKey(dictionaryDto.DictionaryCode))
                {
                    return new ResponseResult<bool>(DbOpStatus.CreateError, "字典Code重复", false);
                }

                if (codeAndNameDic.ContainsValue(dictionaryDto.DictionaryName))
                {
                    return new ResponseResult<bool>(DbOpStatus.CreateError, "字典名称重复", false);
                }

                var entity = FamilyLedgeMapper.Map<DictionaryDto, DictionaryEntity>(dictionaryDto);


                if (string.IsNullOrWhiteSpace(await Repository.AddOneAsync(entity)))
                {
                    return new ResponseResult<bool>(DbOpStatus.CreateError, "新增失败", false);
                }
                return new ResponseResult<bool>(DbOpStatus.Success, true);
            }
            catch (Exception ex)
            {
                return new ResponseResult<bool>(DbOpStatus.CreateError, ex.Message, false);
            }
        }
        #endregion

        #region Update Methods
        public async Task<ReturnResult<bool>> UpdateItemAsync(DictionaryDto dictionaryDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dictionaryDto.DictionaryCode))
                {
                    return new ResponseResult<bool>(DbOpStatus.CreateError, "字典Code不可为空", false);
                }

                if (string.IsNullOrWhiteSpace(dictionaryDto.DictionaryName))
                {
                    return new ResponseResult<bool>(DbOpStatus.CreateError, "字典Code重复", false);
                }

                var allDicEntitys = await Repository.GetEntityListAsync();

                var codeAndNameDic = allDicEntitys.Where(x => x.Id != dictionaryDto.Id).ToDictionary(x => x.DictionaryCode, x => x.DictionaryName);

                if (codeAndNameDic.ContainsKey(dictionaryDto.DictionaryCode))
                {
                    return new ResponseResult<bool>(DbOpStatus.UpdateError, "字典Code重复", false);
                }

                if (codeAndNameDic.ContainsValue(dictionaryDto.DictionaryName))
                {
                    return new ResponseResult<bool>(DbOpStatus.UpdateError, "字典名称重复", false);
                }

                var entity = FamilyLedgeMapper.Map<DictionaryDto, DictionaryEntity>(dictionaryDto);

                if (!await Repository.UpdateOneAsync(entity))
                {
                    return new ResponseResult<bool>(DbOpStatus.UpdateError, "更新失败", false);
                }
                return new ResponseResult<bool>(DbOpStatus.Success, true);
            }
            catch (Exception ex)
            {
                return new ResponseResult<bool>(DbOpStatus.UpdateError, ex.Message, false);
            }
        }
        #endregion

        #region Delete Methods
        public async Task<ReturnResult<bool>> DeleteItemAsync(string id, string userId)
        {
            try
            {
                if (!await Repository.DeleteOneAsync(id, userId))
                {
                    return new ResponseResult<bool>(DbOpStatus.DeleteError, "删除失败", false);
                }
                return new ResponseResult<bool>(DbOpStatus.Success, true);
            }
            catch (Exception ex)
            {
                return new ResponseResult<bool>(DbOpStatus.DeleteError, ex.Message, false);
            }
        }
        #endregion

        #region Other Methods
        public async Task<List<TreeViewItem<DictionaryDto>>> GetTreeViewItemsAsync()
        {
            var result = await GetAllItemsListAsync();
            if (result.Data != null)
            {
                var treeView = Helpers.CascadingDictionaryTree(result.Data).ToList();

                return treeView;
            }
            return new List<TreeViewItem<DictionaryDto>>();
        }

        public async Task<List<SelectedItem>> GetAllSelectedWithoutAllAsync()
        {
            var result = await GetAllItemsListWithoutAllAsync();
            if (result.Data != null)
            {
                var selects = result.Data.Select(x => new SelectedItem(x.Id, x.DictionaryCode)).ToList();
                selects.Insert(0, new SelectedItem("", "请选择"));
                return selects;
            }
            return new List<SelectedItem>();
        }
        #endregion
    }
}
