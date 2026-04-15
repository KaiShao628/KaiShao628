using DatabaseCommon.EntityBase;
using FamilyLedgeManagement.Database;
using MongoDB.Driver;

namespace FamilyLedgeManagement.Repositories
{
    public class DatabaseCollectionBase<T> where T : Entity
    {
        protected readonly IMongoCollection<T> _collection;

        public DatabaseCollectionBase()
        {
            _collection = FamilyLedgeMongoDB.Instance.Register<T>(typeof(T).Name);
        }
    }
}
