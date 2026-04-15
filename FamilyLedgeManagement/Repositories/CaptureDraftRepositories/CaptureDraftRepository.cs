using DatabaseCommon.BaseRepository;
using FamilyLedgeManagement.Database;
using FamilyLedgeManagement.IRepositories.ICaptureDraftRepositories;
using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Repositories.CaptureDraftRepositories
{
    /// <summary>
    /// 截图草稿仓储实现。
    /// </summary>
    public class CaptureDraftRepository : DbBaseRepository<CaptureDraft>, ICaptureDraftRepository
    {
        public CaptureDraftRepository() : base(FamilyLedgeMongoDB.Instance)
        {
        }
    }
}
