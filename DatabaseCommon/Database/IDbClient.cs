using DatabaseCommon.EntityBase;
using DatabaseCommon.MongoDB;
using MongoDB.Driver;
using System.Diagnostics;

namespace DatabaseCommon.Database
{
    public interface IDbClient
    {
        /// <summary>
        /// Start the MongoDB Server.
        Process StartDbServer();

        /// <summary>
        /// Build the database connection and register all entities.
        /// </summary>
        /// <param name="databasePort">The database port.</param>
        /// <param name="databaseName">The database name.</param>
        /// <param name="databaseHost">The database host.</param>
        void BuildConnection(int databasePort, string databaseName, string databaseHost = null, MongodbStartExtraParam extraParam = null);

        /// <summary>
        /// Check if the database server is running.
        /// </summary>
        /// <returns>True if server is running, otherwise is false.</returns>
        bool DatabaseServerIsRunning();

        /// <summary>
        /// Get database collections
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="name">The collection name.</param>
        /// <returns>The data collection</returns>
        IMongoCollection<T> Register<T>(string name) where T : Entity;
    }
}
