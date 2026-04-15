using DatabaseCommon.BaseRepository;
using FamilyLedgeManagement.Models.DictionaryModels;

namespace FamilyLedgeManagement.IRepositories.IDictionaryRepositories
{
    public interface IDictionaryValueRepository : IDbBaseRepository<DictionaryValueEntity>
    {
        Task<Dictionary<string, Tuple<string, string>>> GetDicByDicIdAsync(string dictionaryId, string id = "");
        Task<List<DictionaryValueEntity>> OnlyGetCValueAndValueToSelectedAsync(string dicEntityId);
        Task<bool> UpdateOneAsync(DictionaryValueEntity entity);

        Task<bool> UpdateManyAsync(List<DictionaryValueEntity> entities, string updaterId);

    }
}
