using FamilyLedgeManagement.Enums;
using FamilyLedgeManagement.IRepositories.ITransactionRepositories;
using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Services
{
    /// <summary>
    /// 正式账单数据库业务服务。
    /// </summary>
    public class TransactionService : ServiceBase<ITransactionRepository>
    {
        /// <summary>
        /// 获取全部未删除账单。
        /// </summary>
        public async Task<List<LedgerTransaction>> GetAllAsync()
            => (await Repository.GetEntityListAsync(false))
                .OrderByDescending(x => x.OccurredAt)
                .ToList();

        /// <summary>
        /// 按月份获取账单。
        /// </summary>
        public async Task<List<LedgerTransaction>> GetByMonthAsync(int year, int month)
        {
            var transactions = await Repository.GetEntityListByExpressionAsync(x =>
                !x.IsDeleted &&
                x.OccurredAt.Year == year &&
                x.OccurredAt.Month == month);

            return transactions
                .OrderByDescending(x => x.OccurredAt)
                .ToList();
        }

        /// <summary>
        /// 获取最近账单。
        /// </summary>
        public async Task<List<LedgerTransaction>> GetRecentAsync(int count)
            => (await GetAllAsync()).Take(count).ToList();

        /// <summary>
        /// 根据标识获取账单。
        /// </summary>
        public async Task<LedgerTransaction?> GetByIdAsync(string id)
        {
            var transactions = await Repository.GetEntityListByExpressionAsync(x => !x.IsDeleted && x.Id == id);
            return transactions.FirstOrDefault();
        }

        /// <summary>
        /// 新增账单。
        /// </summary>
        public async Task<string> AddAsync(LedgerTransaction entity)
        {
            entity.CreateTime = DateTime.UtcNow;
            entity.UpdateTime = DateTime.UtcNow;
            entity.IsDeleted = false;
            return await Repository.AddOneAsync(entity);
        }

        /// <summary>
        /// 更新账单。
        /// </summary>
        public async Task<bool> UpdateAsync(LedgerTransaction entity)
        {
            var current = await GetByIdAsync(entity.Id);
            if (current is null)
            {
                return false;
            }

            current.MemberId = entity.MemberId;
            current.CategoryId = entity.CategoryId;
            current.Kind = entity.Kind;
            current.Amount = entity.Amount;
            current.MerchantName = entity.MerchantName;
            current.PaymentMethod = entity.PaymentMethod;
            current.Note = entity.Note;
            current.OccurredAt = entity.OccurredAt;
            current.CreatedAt = entity.CreatedAt == default ? current.CreatedAt : entity.CreatedAt;
            current.UpdateTime = DateTime.UtcNow;

            await Repository.DeleteOneAsync(current.Id, isActualDelete: true);
            return !string.IsNullOrWhiteSpace(await Repository.AddOneAsync(current));
        }

        /// <summary>
        /// 删除账单。
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
            => await Repository.DeleteOneAsync(id, isActualDelete: true);

        /// <summary>
        /// 构建默认账单种子数据。
        /// </summary>
        public IReadOnlyList<LedgerTransaction> BuildDefaultTransactions(DateTimeOffset now) =>
        [
            new()
            {
                Id = Guid.NewGuid().ToString("N"),
                MemberId = "member-kai",
                CategoryId = "cat-food",
                Kind = TransactionKind.Expense,
                Amount = 58,
                MerchantName = "晚餐",
                PaymentMethod = "微信支付",
                Note = "工作日晚餐",
                OccurredAt = now.AddDays(-1),
                CreatedAt = now.AddDays(-1),
                CreateTime = now.UtcDateTime.AddDays(-1),
                UpdateTime = now.UtcDateTime.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid().ToString("N"),
                MemberId = "member-ling",
                CategoryId = "cat-grocery",
                Kind = TransactionKind.Expense,
                Amount = 126.50m,
                MerchantName = "盒马",
                PaymentMethod = "支付宝",
                Note = "周末买菜",
                OccurredAt = now.AddDays(-2),
                CreatedAt = now.AddDays(-2),
                CreateTime = now.UtcDateTime.AddDays(-2),
                UpdateTime = now.UtcDateTime.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid().ToString("N"),
                MemberId = "member-public",
                CategoryId = "cat-home",
                Kind = TransactionKind.Expense,
                Amount = 89.90m,
                MerchantName = "日用品补货",
                PaymentMethod = "银行卡",
                Note = "厨房清洁和纸品",
                OccurredAt = now.AddDays(-4),
                CreatedAt = now.AddDays(-4),
                CreateTime = now.UtcDateTime.AddDays(-4),
                UpdateTime = now.UtcDateTime.AddDays(-4)
            },
            new()
            {
                Id = Guid.NewGuid().ToString("N"),
                MemberId = "member-kai",
                CategoryId = "cat-salary",
                Kind = TransactionKind.Income,
                Amount = 5600m,
                MerchantName = "工资到账",
                PaymentMethod = "银行卡",
                Note = "本月工资",
                OccurredAt = new DateTimeOffset(now.Year, now.Month, 1, 9, 0, 0, now.Offset),
                CreatedAt = new DateTimeOffset(now.Year, now.Month, 1, 9, 0, 0, now.Offset),
                CreateTime = new DateTime(now.Year, now.Month, 1, 1, 0, 0, DateTimeKind.Utc),
                UpdateTime = new DateTime(now.Year, now.Month, 1, 1, 0, 0, DateTimeKind.Utc)
            }
        ];
    }
}
