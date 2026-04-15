using DatabaseCommon.BaseRepository;
using FamilyLedgeManagement.Database;
using FamilyLedgeManagement.IRepositories.ILedgerCategoryRepositories;
using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Repositories.LedgerCategoryRepositories
{
    /// <summary>
    /// 账单分类仓储实现。
    /// </summary>
    public class LedgerCategoryRepository : DbBaseRepository<LedgerCategory>, ILedgerCategoryRepository
    {
        public LedgerCategoryRepository() : base(FamilyLedgeMongoDB.Instance)
        {
        }
    }
}
