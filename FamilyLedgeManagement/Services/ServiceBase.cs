using DatabaseCommon.BaseRepository;
using FamilyLedgeManagement.Database;

namespace FamilyLedgeManagement.Services
{
    public class ServiceBase<T> where T : IDbBaseRepository
    {
        protected T Repository { get; set; }

        public ServiceBase()
        {
             Repository = FamilyLedgeMongoDBClient.Instance.GetRepository<T>();
        }
    }
}
