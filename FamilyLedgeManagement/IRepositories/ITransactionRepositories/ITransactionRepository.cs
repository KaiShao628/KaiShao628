using DatabaseCommon.BaseRepository;
using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.IRepositories.ITransactionRepositories
{
    /// <summary>
    /// 账单仓储接口。
    /// </summary>
    public interface ITransactionRepository : IDbBaseRepository<LedgerTransaction>
    {
    }
}
