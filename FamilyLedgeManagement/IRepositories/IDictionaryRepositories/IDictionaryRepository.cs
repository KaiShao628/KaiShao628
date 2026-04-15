using DatabaseCommon.BaseRepository;
using FamilyLedgeManagement.Models.DictionaryModels;
using System.Collections;

namespace FamilyLedgeManagement.IRepositories.IDictionaryRepositories
{
    public interface IDictionaryRepository : IDbBaseRepository<DictionaryEntity>
    {
        Task<Dictionary<string, string>> GetAllItemsDicToIdCodeAsync();
        Task<string> GetEntityIdByCodeAsync(string code);
        Task<bool> UpdateOneAsync(DictionaryEntity entity);


    }
}
