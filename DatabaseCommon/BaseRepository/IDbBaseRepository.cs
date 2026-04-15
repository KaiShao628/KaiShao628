using DatabaseCommon.EntityBase;
using System.Linq.Expressions;

namespace DatabaseCommon.BaseRepository
{

    public interface IDbBaseRepository
    {

    }

    public interface IDbBaseRepository<TEntity> : IDbBaseRepository where TEntity : Entity
    {
        /// <summary>
        /// 添加数据 - 单条
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<string> AddOneAsync(TEntity entity);

        /// <summary>
        /// 添加数据 - 多条
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<bool> AddManyAsync(List<TEntity> entities);

        /// <summary>
        /// 删除数据 - 单条
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        Task<bool> DeleteOneAsync(string entityId, string userId = "", bool isActualDelete = false);

        /// <summary>
        /// 删除数据 - 多条
        /// </summary>
        /// <param name="entityIds"></param>
        /// <returns></returns>
        Task<bool> DeleteManyAsync(List<string> entityIds, string userId = "", bool isActualDelete = false);

        /// <summary>
        /// 获取数据 - 单条
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        Task<TEntity> GetEntityAsync(string entityId);

        /// <summary>
        /// 获取数据总数
        /// </summary>
        /// <param name="isActualDelete"></param>
        /// <returns></returns>
        Task<long> GetTotalAsync(bool? isActualDelete = false);

        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <param name="isActualDelete"></param>
        /// <returns></returns>
        Task<List<TEntity>> GetEntityListAsync(bool? isActualDelete = false);

        /// <summary>
        /// 获取多条数据
        /// </summary>
        /// <param name="entityIds"></param>
        /// <returns></returns>
        Task<List<TEntity>> GetEntityListByIdsAsync(List<string> entityIds);

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        Task<bool> CheckExistAsync(Expression<Func<TEntity, bool>> expression);

        /// <summary>
        /// 根据表达式获取数据集合
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        Task<List<TEntity>> GetEntityListByExpressionAsync(Expression<Func<TEntity, bool>> expression);

        /// <summary>
        /// 根据表达式获取数据
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        Task<TEntity> GetEntityByExpressionAsync(Expression<Func<TEntity, bool>> expression);

        /// <summary>
        /// 批量获取id
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        Task<List<string>> GetIdListByExpressionAsync(Expression<Func<TEntity, bool>> expression);

        Task<Dictionary<string, TEntity>> GetEntityToIdEntityDicAsync(bool? isDelete = false);
    }
}
