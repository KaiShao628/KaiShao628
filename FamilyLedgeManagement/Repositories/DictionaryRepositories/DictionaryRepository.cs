using DatabaseCommon.BaseRepository;
using DatabaseCommon.Database;
using FamilyLedgeManagement.Database;
using FamilyLedgeManagement.IRepositories.IDictionaryRepositories;
using FamilyLedgeManagement.Models.DictionaryModels;
using MongoDB.Driver;

namespace FamilyLedgeManagement.Repositories.DictionaryRepositories
{
    public class DictionaryRepository : DbBaseRepository<DictionaryEntity>, IDictionaryRepository
    {
        public DictionaryRepository() : base(FamilyLedgeMongoDB.Instance)
        {
        }

        public async Task<Dictionary<string, string>> GetAllItemsDicToIdCodeAsync()
        {
            var allEntitys = await GetEntityListAsync();

            return allEntitys.ToDictionary(x => x.Id, x => x.DictionaryCode);
        }

        public async Task<string> GetEntityIdByCodeAsync(string code)
        {
            var entity = await GetEntityByExpressionAsync(x => !x.IsDeleted && x.DictionaryCode == code);
            if (entity != null)
            {
                return entity.Id;
            }
            return "";
        }


        public async Task<bool> UpdateOneAsync(DictionaryEntity entity)
        {
            var filter = Builders<DictionaryEntity>.Filter.Eq(y => y.Id, entity.Id);

            var update = Builders<DictionaryEntity>.Update.Set(y => y.DictionaryCode, entity.DictionaryCode)
                                                             .Set(y => y.DictionaryName, entity.DictionaryName)
                                                             .Set(y => y.UpdaterId, entity.UpdaterId)
                                                             .Set(y => y.UpdateTime, DateTime.UtcNow);
            await Collection.UpdateOneAsync(filter, update);

            return true;
        }

        public async Task<bool> UpdateManyAsync(List<DictionaryEntity> entities, string updaterId)
        {
            var bulkUpdates = entities.Select(x => new UpdateOneModel<DictionaryEntity>(
                Builders<DictionaryEntity>.Filter.Eq(y => y.Id, x.Id),
                Builders<DictionaryEntity>.Update.Set(y => y.DictionaryCode, x.DictionaryCode)
                                                 .Set(y => y.DictionaryName, x.DictionaryName)
                                                 .Set(y => y.UpdaterId, updaterId)
                                                 .Set(y => y.UpdateTime, DateTime.UtcNow)
                )).ToList();
            await Collection.BulkWriteAsync(bulkUpdates);

            return true;
        }
    }
}
