using DatabaseCommon.BaseRepository;
using FamilyLedgeManagement.Database;
using FamilyLedgeManagement.IRepositories.ITransactionRepositories;
using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Repositories.TransactionRepositories
{
    /// <summary>
    /// 账单仓储实现。
    /// </summary>
    public class TransactionRepository : DbBaseRepository<LedgerTransaction>, ITransactionRepository
    {
        public TransactionRepository() : base(FamilyLedgeMongoDB.Instance)
        {

        }
    }
}
