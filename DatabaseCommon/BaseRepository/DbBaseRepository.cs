using DatabaseCommon.Database;
using DatabaseCommon.EntityBase;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace DatabaseCommon.BaseRepository
{
    public class DbBaseRepository<TEntity> : IDbBaseRepository<TEntity> where TEntity : Entity
    {
        protected readonly IMongoCollection<TEntity> Collection;

        protected static readonly FilterDefinitionBuilder<TEntity> FilterBuilder = Builders<TEntity>.Filter;
        protected static readonly UpdateDefinitionBuilder<TEntity> UpdaterBuilder = Builders<TEntity>.Update;
        protected static readonly SortDefinitionBuilder<TEntity> SortBuilder = Builders<TEntity>.Sort;
        protected static readonly ProjectionDefinitionBuilder<TEntity> ProjectionBuilder = Builders<TEntity>.Projection;

        protected DbBaseRepository(IDbClient dbClient)
        {
            Collection = dbClient.Register<TEntity>(typeof(TEntity).Name);
        }

        /// <summary>
        /// 仅通过 id 构建 filter
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        protected FilterDefinition<TEntity> IdFilter(string entityId) => FilterBuilder.Where(x => x.Id == entityId);

        #region 获取其他实体元素
        protected IMongoCollection<T> GetDbContext<T>(IDbClient dbClient)
            where T : Entity
        {
            return dbClient.Register<T>(typeof(T).Name);
        }

        protected FilterDefinitionBuilder<T> GetFilter<T>()
        {
            return Builders<T>.Filter;
        }

        protected UpdateDefinitionBuilder<T> GetUpdate<T>()
        {
            return Builders<T>.Update;
        }

        protected SortDefinitionBuilder<T> GetSort<T>()
        {
            return Builders<T>.Sort;
        }
        #endregion

        #region 接口实现
        public virtual async Task<bool> AddManyAsync(List<TEntity> entities)
        {
            try
            {
                await Collection.InsertManyAsync(entities);
                return true;
            }
            catch (Exception ex)
            {
                //Logger.WriteLineError(ex.Message);
                return false;
            }
        }

        public virtual async Task<string> AddOneAsync(TEntity entity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entity.Id))
                {
                    entity.Id = Guid.NewGuid().ToString();
                }
                await Collection.InsertOneAsync(entity);
                var model = await GetEntityAsync(entity.Id);
                return model?.Id ?? "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="entityIds">ids</param>
        /// <param name="userId">操作人</param>
        /// <param name="isActualDelete">是否永久删除</param>
        /// <returns></returns>
        public virtual async Task<bool> DeleteManyAsync(List<string> entityIds, string userId = "", bool isActualDelete = false)
        {
            var filter = FilterBuilder.Where(v => entityIds.Contains(v.Id));
            if (isActualDelete)
            {
                var deleteRes = await Collection.DeleteManyAsync(filter);
                return deleteRes.IsAcknowledged;
            }

            var update = UpdaterBuilder.Set(v => v.IsDeleted, true)
                                       .Set(v => v.UpdaterId, userId);
            var res = await Collection.UpdateManyAsync(filter, update);
            return res.IsAcknowledged;
        }

        /// <summary>
        /// 单个删除
        /// </summary>
        /// <param name="entityId">id/param>
        /// <param name="userId">操作人</param>
        /// <param name="isActualDelete">是否永久删除</param>
        public virtual async Task<bool> DeleteOneAsync(string entityId, string userId = "", bool isActualDelete = false)
        {
            if (isActualDelete)
            {
                var deleteRes = await Collection.DeleteOneAsync(IdFilter(entityId));
                return deleteRes.IsAcknowledged;
            }
            var update = UpdaterBuilder.Set(v => v.IsDeleted, true)
                                       .Set(v => v.UpdaterId, userId);
            var res = await Collection.UpdateOneAsync(IdFilter(entityId), update);
            return res.IsAcknowledged;
        }

        /// <summary>
        /// 获取单个实体
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> GetEntityAsync(string entityId)
        {
            return await Collection.Find(IdFilter(entityId)).FirstOrDefaultAsync();
        }


        /// <summary>
        /// ID集合获取实体集合
        /// </summary>
        /// <param name="entityIds"></param>
        /// <returns></returns>
        public virtual async Task<List<TEntity>> GetEntityListByIdsAsync(List<string> entityIds)
        {
            return await Collection.Find(x => entityIds.Contains(x.Id)).ToListAsync();
        }

        /// <summary>
        /// 获取所有实体集合
        /// </summary>
        /// <param name="isDelete"></param>
        /// <returns></returns>
        public virtual async Task<List<TEntity>> GetEntityListAsync(bool? isDelete = false)
        {
            if (isDelete.HasValue) return await Collection.Find(x => x.IsDeleted == isDelete).ToListAsync();
            return await Collection.Find(x => true).ToListAsync();
        }

        /// <summary>
        /// 根据条件获取实体集合
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<List<TEntity>> GetEntityListByExpressionAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await Collection.Find(expression).ToListAsync();
        }

        public async Task<TEntity> GetEntityByExpressionAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await Collection.Find(expression).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 根据条件获取实体ID集合
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<List<string>> GetIdListByExpressionAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await Collection.Find(expression).Project(x => x.Id).ToListAsync();
        }

        public async Task<Dictionary<string, TEntity>> GetEntityToIdEntityDicAsync(bool? isDelete = false)
        {
            if (isDelete.HasValue)
            {
                var noDelEntityList = await Collection.Find(x => x.IsDeleted == isDelete).ToListAsync();

                return noDelEntityList.ToDictionary(x => x.Id, x => x);
            }
            var allEntityList = await Collection.Find(x => true).ToListAsync();
            return allEntityList.ToDictionary(x => x.Id, x => x);
        }


        public virtual async Task<long> GetTotalAsync(bool? isDelete = false)
        {
            if (isDelete.HasValue) return await Collection.CountDocumentsAsync(x => x.IsDeleted == isDelete);
            return await Collection.CountDocumentsAsync(x => true);
        }


        public async Task<bool> CheckExistAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await Collection.Find(expression).AnyAsync();
        }

        #endregion
    }
}
