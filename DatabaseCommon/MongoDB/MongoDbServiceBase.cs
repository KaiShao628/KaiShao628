using DatabaseCommon.BaseService;
using DatabaseCommon.Database;
using DatabaseCommon.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DatabaseCommon.MongoDB
{
    public abstract class MongoDbServiceBase : DbServiceBase
    {
        /// <summary>
        /// Initialize the mongodb database service.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="databasePort">The database port.</param>
        /// <param name="databaseName">The database name.</param>
        /// <param name="logDatabasePort">The log databse port.</param>
        /// <param name="logDatabaseName">The log database name.</param>
        /// <param name="databaseHost">The databse host.</param>
        public override void Initialize(IDbClient dbClient, int databasePort, string databaseName, string databaseHost = null, MongodbStartExtraParam extraParam = null)
        {
            _ = InitializeWithProcessReturned(dbClient, databasePort, databaseName, databaseHost, extraParam);
        }

        public StartDBResult InitializeWithProcessReturned(IDbClient dbClient, int databasePort, string databaseName, string databaseHost = null, MongodbStartExtraParam extraParam = null)
        {
            extraParam ??= new();

            StartDBResult result = new();
            var tryTimes = 3;
            dbClient.BuildConnection(databasePort, databaseName, databaseHost, extraParam);
            while (!dbClient.DatabaseServerIsRunning() && tryTimes > 0)
            {
                result.DBProcess = dbClient.StartDbServer();
                tryTimes--;
                Thread.Sleep(1000);
                dbClient.BuildConnection(databasePort, databaseName, databaseHost);
            }
            if (!dbClient.DatabaseServerIsRunning())
            {
                LogHelper.WriteLineError("Database server is not running.");
                throw new InvalidOperationException("Database server is not running.");
            }

            RegisterRepositoris();
            InitializeDBIndexes();
            PrepareDefaultData();
            LogHelper.WriteLineInfo("Database service initialized.");
            return result;
        }

        public class StartDBResult
        {
            public Process LogDBProcess { get; set; }
            public Process DBProcess { get; set; }

            public bool FinishProcess()
            {
                try
                {
                    if (LogDBProcess is not null && !LogDBProcess.HasExited)
                    {
                        LogDBProcess.Kill();
                    }
                }
                catch (Exception ex)
                {

                }
                try
                {
                    if (DBProcess is not null && !DBProcess.HasExited)
                    {
                        DBProcess.Kill();
                    }
                }
                catch (Exception ex)
                {

                }
                return true;
            }
        }
    }

    public class MongodbStartExtraParam
    {
        /// <summary>
        /// "rs0"
        /// </summary>
        public string ReplSet { get; set; }
        public int CacheSizeGB { get; set; } = -1;
    }
}
