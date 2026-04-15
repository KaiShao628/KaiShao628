using DatabaseCommon.BaseRepository;
using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.IRepositories.IFamilyMemberRepositories
{
    /// <summary>
    /// 家庭成员仓储接口。
    /// </summary>
    public interface IFamilyMemberRepository : IDbBaseRepository<FamilyMember>
    {
    }
}
