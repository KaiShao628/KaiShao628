using DatabaseCommon.MongoDB;
using FamilyLedgeManagement.IRepositories.ICaptureDraftRepositories;
using FamilyLedgeManagement.IRepositories.IDictionaryRepositories;
using FamilyLedgeManagement.IRepositories.IFamilyMemberRepositories;
using FamilyLedgeManagement.IRepositories.ILedgerCategoryRepositories;
using FamilyLedgeManagement.IRepositories.ITransactionRepositories;
using FamilyLedgeManagement.Models;
using FamilyLedgeManagement.Repositories.CaptureDraftRepositories;
using FamilyLedgeManagement.Repositories.DictionaryRepositories;
using FamilyLedgeManagement.Repositories.FamilyMemberRepositories;
using FamilyLedgeManagement.Repositories.LedgerCategoryRepositories;
using FamilyLedgeManagement.Repositories.TransactionRepositories;
using FamilyLedgeManagement.Utilities;

namespace FamilyLedgeManagement.Database
{
    /// <summary>
    /// 项目 MongoDB 服务入口，负责初始化数据库并注册仓储。
    /// </summary>
    internal class FamilyLedgeMongoDBClient : MongoDbServiceBase
    {
        public static FamilyLedgeMongoDBClient Instance { get; set; } = new FamilyLedgeMongoDBClient();

        private string _databaseHost = string.Empty;
        private int _databasePort;
        private readonly string _databaseName = "KLFamilyLedge";

        /// <summary>
        /// 启动数据库连接。
        /// </summary>
        public void StartServer()
        {
            _databaseHost = KLFamilyLedgeAppSettingsHelper.KLFamilyLedgeAppSettings.DataBase.Host;
            _databasePort = KLFamilyLedgeAppSettingsHelper.KLFamilyLedgeAppSettings.DataBase.Port;
            base.Initialize(FamilyLedgeMongoDB.Instance, _databasePort, _databaseName, _databaseHost);
        }

        protected override void RegisterRepositoris()
        {
            RegisterRepository<IFamilyMemberRepository>(new FamilyMemberRepository());
            RegisterRepository<ILedgerCategoryRepository>(new LedgerCategoryRepository());
            RegisterRepository<ICaptureDraftRepository>(new CaptureDraftRepository());
            RegisterRepository<ITransactionRepository>(new TransactionRepository());
            RegisterRepository<IDictionaryRepository>(new DictionaryRepository());
            RegisterRepository<IDictionaryValueRepository>(new DictionaryValueRepository());
        }

        protected override void InitializeDBIndexes()
        {
        }

        //初始化默认数据，如默认账单分类等
        protected override async void PrepareDefaultData()
        {
            var captureDraftRepo = GetRepository<ICaptureDraftRepository>();
            var familyMemberRepo = GetRepository<IFamilyMemberRepository>();
            var transactionRepo = GetRepository<ITransactionRepository>();
            var categoryRepo = GetRepository<ILedgerCategoryRepository>();




            var defaultCategories = new[]
            {
                new LedgerCategory { Name = "餐饮", Icon = "🍽️" },
                new LedgerCategory { Name = "交通", Icon = "🚗" },
                new LedgerCategory { Name = "购物", Icon = "🛍️" },
                new LedgerCategory { Name = "娱乐", Icon = "🎮" },
                new LedgerCategory { Name = "医疗", Icon = "💊" },
                new LedgerCategory { Name = "教育", Icon = "📚" },
                new LedgerCategory { Name = "其他", Icon = "🔖" }
            };
        }
    }
}
