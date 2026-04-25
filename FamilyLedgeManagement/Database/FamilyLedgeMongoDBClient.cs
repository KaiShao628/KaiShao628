using DatabaseCommon.MongoDB;
using FamilyLedgeManagement.IRepositories.IDictionaryRepositories;
using FamilyLedgeManagement.IRepositories.IFamilyMemberRepositories;
using FamilyLedgeManagement.IRepositories.ILedgerCategoryRepositories;
using FamilyLedgeManagement.IRepositories.ITransactionRepositories;
using FamilyLedgeManagement.Models;
using FamilyLedgeManagement.Models.DictionaryModels;
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
            await InitDictionaryAndDicValue();
        }

        /// <summary>
        /// 初始化字典和字典值
        /// </summary>
        /// <returns></returns>
        private async Task InitDictionaryAndDicValue()
        {
            var dicResp = GetRepository<IDictionaryRepository>();
            var allDics = await dicResp.GetEntityListAsync();
            var dicValResp = GetRepository<IDictionaryValueRepository>();
            var allDicValues = await dicValResp.GetEntityListAsync();
            if (!allDics.Any() && !allDicValues.Any())
            {
                var allDicEntity = CreateDictionaryEntity("全部", "全部");

                var transactionKind = CreateDictionaryEntity("TransactionKind", "账单收支类型");
                //var projectPhase = CreateDictionaryEntity("ProjectPhase", "项目阶段");
                //var timeSheetStatus = CreateDictionaryEntity("TimeSheetStatus", "工时填报状态");
                //var workHourStatisticStatus = CreateDictionaryEntity("HourStatisticStatus", "工时上报状态");
                //var projectStatus = CreateDictionaryEntity("ProjectStatus", "项目状态");
                //var taskType = CreateDictionaryEntity("TaskType", "任务类型");
                //var taskStatus = CreateDictionaryEntity("TaskStatus", "任务状态");
                //var priority = CreateDictionaryEntity("Priority", "优先级");

                var dicResult = await dicResp.AddManyAsync(new List<DictionaryEntity> { allDicEntity, transactionKind });
                if (dicResult)
                {
                    FamilyLedgeServerConsoleHelper.GreenLog($"日期：{DateTime.Now:yyyy-MM-dd HH:mm:ss},初始化字典{allDicEntity.DictionaryName}完成");
                }

                var expense = CreateDictionaryValueEntity("支出", "Expense", transactionKind.Id);
                var income = CreateDictionaryValueEntity("收入", "Income", transactionKind.Id);
                //var confirmed = CreateDictionaryValueEntity("已确认", "Confirmed", timeSheetStatus.Id);
                //var toBeConfirmed = CreateDictionaryValueEntity("待确认", "ToBeConfirmed", timeSheetStatus.Id);
                //var toBeUpdate = CreateDictionaryValueEntity("待更新", "ToBeUpdate", timeSheetStatus.Id);


                var divValueResult = await dicValResp.AddManyAsync(new List<DictionaryValueEntity> { expense, income });
                if (divValueResult)
                {
                    FamilyLedgeServerConsoleHelper.GreenLog($"日期：{DateTime.Now:yyyy-MM-dd HH:mm:ss},初始化字典工时填报状态的字典值完成");
                }

            }
        }

        private static DictionaryEntity CreateDictionaryEntity(string code, string name)
        {
            return new DictionaryEntity
            {
                CreatorId = "System",
                CreateTime = DateTime.UtcNow,
                DictionaryCode = code,
                DictionaryName = name,
                IsSystem = true,
                Id = Guid.NewGuid().ToString(),
                IsDeleted = false,
            };
        }

        private static DictionaryValueEntity CreateDictionaryValueEntity(string cValue, string value, string dicId = "")
        {
            return new DictionaryValueEntity
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.UtcNow,
                CreatorId = "System",
                CValue = cValue,
                DictionaryId = dicId ?? "",
                IsDeleted = false,
                IsUsed = true,
                IsSystem = true,
                Code = value,
                Value = value
            };
        }
    }
}
