using FamilyLedgeManagement.Database;
using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Dtos.BaseDtos;
using FamilyLedgeManagement.Dtos.DictionaryDtos;
using FamilyLedgeManagement.Enums;
using FamilyLedgeManagement.IRepositories.IDictionaryRepositories;
using FamilyLedgeManagement.Models;
using FamilyLedgeManagement.Models.DictionaryModels;
using FamilyLedgeManagement.Utilities;

namespace FamilyLedgeManagement.Services.DictionaryServices
{
    public class DictionaryValueService : ServiceBase<IDictionaryValueRepository>
    {
        private readonly IDictionaryRepository _dictionaryRepository = FamilyLedgeMongoDBClient.Instance.GetRepository<IDictionaryRepository>();
        #region Get Methods
        public async Task<ReturnResult<TableResultDto<DictionaryValueDto>>> OnQueryAsync(FilterDto filterDto, string dicId = "")
        {
            try
            {
                var totalPages = 0;
                var allDicList = await _dictionaryRepository.GetAllItemsDicToIdCodeAsync();

                var dicValueEntityList = await Repository.GetEntityListAsync();

                if (!string.IsNullOrWhiteSpace(filterDto.DicId))
                {
                    dicValueEntityList = dicValueEntityList.Where(x => x.DictionaryId == filterDto.DicId).ToList();
                }

                if (!string.IsNullOrWhiteSpace(filterDto.SearchText))
                {
                    dicValueEntityList = dicValueEntityList.Where(x => x.CValue.Contains(filterDto.SearchText.Trim(), StringComparison.OrdinalIgnoreCase) ||
                                                                       x.Value.Contains(filterDto.SearchText.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

                }
                totalPages = dicValueEntityList.Count;

                var paginationList = dicValueEntityList.OrderByDescending(x => x.CreateTime).Skip((filterDto.PageIndex - 1) * filterDto.PageSize).Take(filterDto.PageSize).ToList();

                var dtoList = FamilyLedgeMapper.MapList<DictionaryValueEntity, DictionaryValueDto>(paginationList);

                dtoList = dtoList.Select(x =>
                {
                    x.DictionaryName = allDicList.TryGetValue(x.DictionaryId, out var dicName) ? dicName : "";
                    return x;
                }).ToList();

                var result = new TableResultDto<DictionaryValueDto>()
                {
                    Result = dtoList,
                    TotalPages = totalPages,
                };

                return new ResponseResult<TableResultDto<DictionaryValueDto>>(DbOpStatus.Success, result);
            }
            catch (Exception ex)
            {
                return new ResponseResult<TableResultDto<DictionaryValueDto>>(DbOpStatus.QueryError, ex.Message, null);
            }
        }

        public async Task<ReturnResult<List<DictionaryValueDto>>> GetDicValueByCodeAsync(string code)
        {
            try
            {
                var dicEntityId = await _dictionaryRepository.GetEntityIdByCodeAsync(code);
                if (string.IsNullOrWhiteSpace(dicEntityId))
                {
                    return new ResponseResult<List<DictionaryValueDto>>(DbOpStatus.QueryError, $"无法找到该字典:{code}", null);
                }

                var dicValueList = await Repository.OnlyGetCValueAndValueToSelectedAsync(dicEntityId);

                var dtoList = FamilyLedgeMapper.MapList<DictionaryValueEntity, DictionaryValueDto>(dicValueList);

                return new ResponseResult<List<DictionaryValueDto>>(DbOpStatus.Success, dtoList);
            }
            catch (Exception ex)
            {
                return new ResponseResult<List<DictionaryValueDto>>(DbOpStatus.QueryError, ex.Message, null);
            }
        }


        public async Task<string> GetItemIdByValueAsync(string value)
        {
            var entity = await Repository.GetEntityByExpressionAsync(x => !x.IsDeleted && x.IsUsed && x.Value == value);

            return entity?.Id;
        }

        public async Task<ReturnResult<List<DictionaryValueDto>>> GetItemsAsync()
        {
            try
            {
                var entityList = await Repository.GetEntityListAsync();

                var dtoList = FamilyLedgeMapper.MapList<DictionaryValueEntity, DictionaryValueDto>(entityList);

                return new ResponseResult<List<DictionaryValueDto>>(DbOpStatus.Success, dtoList);
            }
            catch (Exception ex)
            {
                return new ResponseResult<List<DictionaryValueDto>>(DbOpStatus.QueryError, ex.Message, null);
            }
        }

        public async Task<ReturnResult<List<string>>> GetTimesheetNeedOperationAsync()
        {
            var allDicValue = await Repository.GetEntityListAsync();

            var result = allDicValue.Where(x => x.CValue == "待确认" || x.CValue == "待撤回").Select(x => x.Id).ToList();

            return new ResponseResult<List<string>>(DbOpStatus.Success, result);

        }

        public async Task<ReturnResult<DictionaryValueDto>> GetItemByValueAsync(string value)
        {
            try
            {
                var entity = await Repository.GetEntityByExpressionAsync(x => !x.IsDeleted && x.IsUsed && x.Value == value);
                var dto = FamilyLedgeMapper.Map<DictionaryValueEntity, DictionaryValueDto>(entity);
                return new ResponseResult<DictionaryValueDto>(DbOpStatus.Success, dto);
            }
            catch (Exception ex)
            {
                return new ResponseResult<DictionaryValueDto>(DbOpStatus.QueryError, ex.Message, null);
            }

        }
        #endregion

        #region Add Methods
        public async Task<ReturnResult<bool>> OnAddAsync(DictionaryValueDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Value))
                {
                    return new ResponseResult<bool>(DbOpStatus.CreateError, "值不可为空", false);
                }

                if (string.IsNullOrWhiteSpace(dto.CValue))
                {
                    return new ResponseResult<bool>(DbOpStatus.CreateError, "中文值不可为空", false);
                }

                if (string.IsNullOrWhiteSpace(dto.DictionaryId))
                {
                    return new ResponseResult<bool>(DbOpStatus.CreateError, "所属字典不可为空", false);
                }

                var valueDic = await Repository.GetDicByDicIdAsync(dto.DictionaryId);
                if (valueDic.Any())
                {
                    if (valueDic.ContainsKey(dto.Value))
                    {
                        return new ResponseResult<bool>(DbOpStatus.CreateError, "值重复", false);
                    }

                    if (valueDic.Values.Any(x => x.Item1 == dto.CValue))
                    {
                        return new ResponseResult<bool>(DbOpStatus.CreateError, "中文值重复", false);
                    }

                    if (valueDic.Values.Any(x => x.Item2 == dto.Code))
                    {
                        return new ResponseResult<bool>(DbOpStatus.CreateError, "Code重复", false);
                    }
                }
                var entity = FamilyLedgeMapper.Map<DictionaryValueDto, DictionaryValueEntity>(dto);

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
        public async Task<ReturnResult<bool>> OnEditAsync(DictionaryValueDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Value))
                {
                    return new ResponseResult<bool>(DbOpStatus.CreateError, "值不可为空", false);
                }

                if (string.IsNullOrWhiteSpace(dto.CValue))
                {
                    return new ResponseResult<bool>(DbOpStatus.CreateError, "中文值不可为空", false);
                }

                if (string.IsNullOrWhiteSpace(dto.DictionaryId))
                {
                    return new ResponseResult<bool>(DbOpStatus.CreateError, "所属字典不可为空", false);
                }
                var valueDic = await Repository.GetDicByDicIdAsync(dto.DictionaryId, dto.Id);

                if (valueDic.Any())
                {
                    if (valueDic.ContainsKey(dto.Value))
                    {
                        return new ResponseResult<bool>(DbOpStatus.CreateError, "值重复", false);
                    }

                    if (valueDic.Values.Any(x => x.Item1 == dto.CValue))
                    {
                        return new ResponseResult<bool>(DbOpStatus.CreateError, "中文值重复", false);
                    }

                    if (valueDic.Values.Any(x => x.Item2 == dto.Code))
                    {
                        return new ResponseResult<bool>(DbOpStatus.CreateError, "Code重复", false);
                    }
                }

                var entity = FamilyLedgeMapper.Map<DictionaryValueDto, DictionaryValueEntity>(dto);

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
        public async Task<ReturnResult<bool>> OnDeleteItemsAsync(IEnumerable<string> ids, string userId)
        {
            try
            {
                if (!await Repository.DeleteManyAsync(ids.ToList(), userId))
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
    }
}
