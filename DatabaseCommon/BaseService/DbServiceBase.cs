using DatabaseCommon.BaseRepository;
using DatabaseCommon.Database;
using DatabaseCommon.MongoDB;
using DatabaseCommon.Utilities;

namespace DatabaseCommon.BaseService
{
    public abstract class DbServiceBase : IDbServiceBase
    {
        private readonly Dictionary<Type, IDbBaseRepository> _repositories = new Dictionary<Type, IDbBaseRepository>();

        /// <summary>
        /// Initialize the database service.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="databasePort">The database port.</param>
        /// <param name="databaseName">The database name.</param>
        /// <param name="logDatabasePort">The log databse port.</param>
        /// <param name="logDatabaseName">The log database name.</param>
        /// <param name="databaseHost">The databse host.</param>
        public abstract void Initialize(IDbClient dbClient, int databasePort, string databaseName, string databaseHost = null, MongodbStartExtraParam extraParam = null);

        /// <summary>
        /// Get one database table repository.
        /// </summary>
        /// <typeparam name="T">The type of the database table handler.</typeparam>
        /// <returns>The database table handler</returns>
        public T GetRepository<T>() where T : IDbBaseRepository
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                LogHelper.WriteLineError($"{type} does not exist in DatabaseService.");
                throw new KeyNotFoundException($"{type} does not exist in DatabaseService.");
            }
            return (T)_repositories[type];
        }

        /// <summary>
        /// Register a manager into the service.
        /// </summary>
        /// <typeparam name="T">The type of the manager.</typeparam>
        /// <param name="manager">The repository instance</param>
        protected void RegisterRepository<T>(T repository) where T : IDbBaseRepository
        {
            var type = typeof(T);
            if (_repositories.ContainsKey(type))
            {
                throw new InvalidOperationException($"{type} already exists in DatabaseService.");
            }
            _repositories.Add(type, repository);
            LogHelper.WriteLineInfo($"Registered repository {repository.GetType().Name} as {typeof(T).Name}.");
        }

        protected abstract void RegisterRepositoris();

        protected abstract void InitializeDBIndexes();

        protected abstract void PrepareDefaultData();
    }
}
