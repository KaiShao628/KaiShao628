using DatabaseCommon.BaseRepository;
using FamilyLedgeManagement.Database;
using FamilyLedgeManagement.IRepositories.IFamilyMemberRepositories;
using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Repositories.FamilyMemberRepositories
{
    /// <summary>
    /// 家庭成员仓储实现。
    /// </summary>
    public class FamilyMemberRepository : DbBaseRepository<FamilyMember>, IFamilyMemberRepository
    {
        public FamilyMemberRepository() : base(FamilyLedgeMongoDB.Instance)
        {
        }
    }
}
