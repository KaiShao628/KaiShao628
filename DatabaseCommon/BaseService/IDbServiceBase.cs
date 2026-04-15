using DatabaseCommon.BaseRepository;
using DatabaseCommon.Database;
using DatabaseCommon.MongoDB;

namespace DatabaseCommon.BaseService
{
    public interface IDbServiceBase
    { /// <summary>
      /// Intialize database service.
      /// </summary>
      /// <param name="dbClient">The database client.</param>
      /// <param name="databasePort">The database port.</param>
      /// <param name="databaseName">The database name.</param>
      /// <param name="logDatabasePort">The log databse port.</param>
      /// <param name="logDatabaseName">The log database name.</param>
      /// <param name="databaseHost">The databse host.</param>
        void Initialize(IDbClient dbClient, int databasePort, string databaseName, string databaseHost = null, MongodbStartExtraParam extraParam = null);

        /// <summary>
        /// Get one database table repository.
        /// </summary>
        /// <typeparam name="T">The type of the database table handler.</typeparam>
        /// <returns>The database table handler</returns>
        T GetRepository<T>() where T : IDbBaseRepository;
    }
}
