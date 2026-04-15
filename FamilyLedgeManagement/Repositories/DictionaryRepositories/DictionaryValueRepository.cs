using DatabaseCommon.BaseRepository;
using FamilyLedgeManagement.Database;
using FamilyLedgeManagement.IRepositories.IDictionaryRepositories;
using FamilyLedgeManagement.Models.DictionaryModels;
using MongoDB.Driver;

namespace FamilyLedgeManagement.Repositories.DictionaryRepositories
{
    public class DictionaryValueRepository : DbBaseRepository<DictionaryValueEntity>, IDictionaryValueRepository
    {
        public DictionaryValueRepository() : base(FamilyLedgeMongoDB.Instance)
        {
        }


        public async Task<List<DictionaryValueEntity>> OnlyGetCValueAndValueToSelectedAsync(string dicEntityId)
        {
            var filter = FilterBuilder.Where(x => !x.IsDeleted && x.IsUsed && x.DictionaryId == dicEntityId);
            var project = ProjectionBuilder.Include(x => x.CValue).Include(x => x.Value);

            return await Collection.Find(filter).Project<DictionaryValueEntity>(project).ToListAsync();
        }

        public async Task<Dictionary<string, Tuple<string, string>>> GetDicByDicIdAsync(string dictionaryId, string id)
        {
            var entitys = new List<DictionaryValueEntity>();
            if (string.IsNullOrWhiteSpace(id))
            {
                entitys = await GetEntityListByExpressionAsync(x => !x.IsDeleted && x.DictionaryId == dictionaryId);
            }
            else
            {
                entitys = await GetEntityListByExpressionAsync(x => !x.IsDeleted && x.DictionaryId == dictionaryId && x.Id != id);
            }

            return entitys.ToDictionary(x => x.Value, x => new Tuple<string, string>(x.CValue, x.Code));
        }

        public async Task<bool> UpdateOneAsync(DictionaryValueEntity entity)
        {
            var filter = Builders<DictionaryValueEntity>.Filter.Eq(y => y.Id, entity.Id);

            var update = Builders<DictionaryValueEntity>.Update.Set(y => y.Value, entity.Value)
                                                               .Set(y => y.CValue, entity.CValue)
                                                               .Set(y => y.IsUsed, entity.IsUsed)
                                                               .Set(y => y.UpdaterId, entity.UpdaterId)
                                                               .Set(y => y.UpdateTime, DateTime.UtcNow);
            await Collection.UpdateOneAsync(filter, update);

            return true;
        }

        public async Task<bool> UpdateManyAsync(List<DictionaryValueEntity> entities, string updaterId)
        {
            var bulkUpdates = entities.Select(x => new UpdateOneModel<DictionaryValueEntity>(
                Builders<DictionaryValueEntity>.Filter.Eq(y => y.Id, x.Id),
                Builders<DictionaryValueEntity>.Update.Set(y => y.CValue, x.CValue)
                                                      .Set(y => y.Value, x.Value)
                                                      .Set(y => y.IsUsed, x.IsUsed)
                                                      .Set(y => y.UpdaterId, updaterId)
                                                      .Set(y => y.UpdateTime, DateTime.UtcNow)
                )).ToList();
            await Collection.BulkWriteAsync(bulkUpdates);

            return true;
        }
    }
}
