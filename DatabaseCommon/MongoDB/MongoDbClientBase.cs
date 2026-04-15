using DatabaseCommon.Database;
using DatabaseCommon.EntityBase;
using DatabaseCommon.Utilities;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DatabaseCommon.MongoDB
{
    public class MongoDbClientBase : IDbClient
    {
        private const int MaxConnections = 1024;

        private const int ConnectTimeOut = 3;
        private const int WaitQueueTimeout = 3;
        private const int ServerSelectionTimeout = 20;
        private const int SocketTimeout = 20;

        private static string _databaseHost = "127.0.0.1";
        private static string _databaseName;
        private static int _databasePort;
        private static int _cacheSizeGB;
        private static string _replSet;

        private static int _logDatabasePort;
        private MongoClient _databaseClient;
        private MongoClient _logDatabaseClient;
        private IMongoDatabase _database;
        private IMongoDatabase _logDatabase;

        public Process StartDbServer()
        {
            Process process = null;

            LogHelper.WriteLineInfo("Starting database server...");
            var mongod = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "mongod");
            var dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "Database");
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }
            try
            {
                var arguments = $"--dbpath \"{dataFolder}\" --port {_databasePort} --bind_ip 0.0.0.0 --maxConns {MaxConnections}";
                if (_cacheSizeGB > 0)
                {
                    arguments += $" --wiredTigerCacheSizeGB {_cacheSizeGB}";
                }
                if (!string.IsNullOrWhiteSpace(_replSet))
                {
                    arguments += $" --replSet {_replSet}";
                }
                else
                {
                    arguments += $" --noIndexBuildRetry";
                }
                var startInfo = new ProcessStartInfo
                {
                    FileName = mongod,
                    Arguments = arguments,
                };
                process = Process.Start(startInfo);
                while (!DatabaseServerIsRunning())
                {
                    Thread.Sleep(1000);
                }
                LogHelper.WriteLineInfo("Mongodb server started.");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLineError($"Start mongod server process failed,error:{ex}");
                throw;
            }
            return process;
        }

        public void BuildConnection(int databasePort, string databaseName, string databaseHost = null, MongodbStartExtraParam extraParam = null)
        {
            extraParam ??= new();
            if (!string.IsNullOrEmpty(databaseHost))
            {
                _databaseHost = databaseHost;
            }
            _databasePort = databasePort;
            _databaseName = databaseName;
            _cacheSizeGB = extraParam.CacheSizeGB;
            _replSet = extraParam.ReplSet;
            BuilderClient();
        }

        public bool DatabaseServerIsRunning()
        {
            try
            {
                if (_database == null)
                {
                    return false;
                }
                return _database.Client.Cluster.Description.State == ClusterState.Connected;
            }
            catch
            {
                // ignored
            }

            return false;
        }

        public IMongoCollection<T> Register<T>(string name) where T : Entity
        {
            return _database.GetCollection<T>(name);
        }

        protected virtual void RegisterEntities() { }

        private void BuilderClient()
        {
            var setting = new MongoClientSettings
            {
                ConnectTimeout = TimeSpan.FromSeconds(ConnectTimeOut),
                Server = new MongoServerAddress(_databaseHost, _databasePort),
                MaxConnectionPoolSize = MaxConnections,
                WaitQueueTimeout = TimeSpan.FromSeconds(WaitQueueTimeout),
                ServerSelectionTimeout = TimeSpan.FromSeconds(ServerSelectionTimeout),
                SocketTimeout = TimeSpan.FromSeconds(SocketTimeout)
            };
            _databaseClient = new MongoClient(setting);
            _database = _databaseClient.GetDatabase(_databaseName);
        }
    }
}
