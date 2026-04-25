using FamilyLedgeManagement.Enums;
using FamilyLedgeManagement.IRepositories.ILedgerCategoryRepositories;
using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Services
{
    /// <summary>
    /// 账单分类数据库业务服务。
    /// </summary>
    public class LedgerCategoryService : ServiceBase<ILedgerCategoryRepository>
    {
        public async Task<List<LedgerCategory>> GetAllAsync()
            => (await Repository.GetEntityListAsync(false))
                .OrderBy(x => x.Kind)
                .ThenBy(x => x.Name)
                .ToList();

        public async Task<LedgerCategory?> GetByIdAsync(string id)
        {
            var categories = await Repository.GetEntityListByExpressionAsync(x => !x.IsDeleted && x.Id == id);
            return categories.FirstOrDefault();
        }

        public async Task<bool> CheckNameExistsAsync(string name, string kind, string? excludeId = null)
            => await Repository.CheckExistAsync(x =>
                !x.IsDeleted &&
                x.Kind == kind &&
                x.Name.ToLower() == name.ToLower() &&
                (string.IsNullOrWhiteSpace(excludeId) || x.Id != excludeId));

        public async Task<string> AddAsync(LedgerCategory entity)
        {
            entity.CreateTime = DateTime.UtcNow;
            entity.UpdateTime = DateTime.UtcNow;
            entity.IsDeleted = false;
            return await Repository.AddOneAsync(entity);
        }

        public async Task<bool> UpdateAsync(LedgerCategory entity)
        {
            var current = await GetByIdAsync(entity.Id);
            if (current is null)
            {
                return false;
            }

            current.Name = entity.Name;
            current.Kind = entity.Kind;
            current.Color = entity.Color;
            current.UpdateTime = DateTime.UtcNow;

            await Repository.DeleteOneAsync(current.Id, isActualDelete: true);
            return !string.IsNullOrWhiteSpace(await Repository.AddOneAsync(current));
        }

        public async Task<bool> DeleteAsync(string id)
            => await Repository.DeleteOneAsync(id, isActualDelete: true);

        //public IReadOnlyList<LedgerCategory> BuildDefaultCategories() =>
        //[
        //    new() { Id = "cat-food", Name = "餐饮", Kind = TransactionKind.Expense, Color = "#dc6e2f", CreateTime = DateTime.UtcNow, UpdateTime = DateTime.UtcNow },
        //    new() { Id = "cat-grocery", Name = "买菜", Kind = TransactionKind.Expense, Color = "#ef8f3d", CreateTime = DateTime.UtcNow, UpdateTime = DateTime.UtcNow },
        //    new() { Id = "cat-traffic", Name = "交通", Kind = TransactionKind.Expense, Color = "#245b90", CreateTime = DateTime.UtcNow, UpdateTime = DateTime.UtcNow },
        //    new() { Id = "cat-home", Name = "家居日用", Kind = TransactionKind.Expense, Color = "#2f7d72", CreateTime = DateTime.UtcNow, UpdateTime = DateTime.UtcNow },
        //    new() { Id = "cat-entertainment", Name = "娱乐", Kind = TransactionKind.Expense, Color = "#7d4c8f", CreateTime = DateTime.UtcNow, UpdateTime = DateTime.UtcNow },
        //    new() { Id = "cat-salary", Name = "工资", Kind = TransactionKind.Income, Color = "#24745c", CreateTime = DateTime.UtcNow, UpdateTime = DateTime.UtcNow },
        //    new() { Id = "cat-bonus", Name = "红包", Kind = TransactionKind.Income, Color = "#4a9d67", CreateTime = DateTime.UtcNow, UpdateTime = DateTime.UtcNow }
        //];
    }
}
