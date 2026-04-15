using FamilyLedgeManagement.IRepositories.IFamilyMemberRepositories;
using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Services
{
    /// <summary>
    /// 家庭成员数据库业务服务。
    /// </summary>
    public class FamilyMemberService : ServiceBase<IFamilyMemberRepository>
    {
        public async Task<List<FamilyMember>> GetAllAsync()
            => (await Repository.GetEntityListAsync(false))
                .OrderBy(x => x.Name)
                .ToList();

        public async Task<FamilyMember?> GetByIdAsync(string id)
        {
            var members = await Repository.GetEntityListByExpressionAsync(x => !x.IsDeleted && x.Id == id);
            return members.FirstOrDefault();
        }

        public async Task<bool> CheckNameExistsAsync(string name, string? excludeId = null)
            => await Repository.CheckExistAsync(x =>
                !x.IsDeleted &&
                x.Name.ToLower() == name.ToLower() &&
                (string.IsNullOrWhiteSpace(excludeId) || x.Id != excludeId));

        public async Task<string> AddAsync(FamilyMember entity)
        {
            entity.CreateTime = DateTime.UtcNow;
            entity.UpdateTime = DateTime.UtcNow;
            entity.IsDeleted = false;
            return await Repository.AddOneAsync(entity);
        }

        public async Task<bool> UpdateAsync(FamilyMember entity)
        {
            var current = await GetByIdAsync(entity.Id);
            if (current is null)
            {
                return false;
            }

            current.Name = entity.Name;
            current.Role = entity.Role;
            current.AccentColor = entity.AccentColor;
            current.UpdateTime = DateTime.UtcNow;

            await Repository.DeleteOneAsync(current.Id, isActualDelete: true);
            return !string.IsNullOrWhiteSpace(await Repository.AddOneAsync(current));
        }

        public async Task<bool> DeleteAsync(string id)
            => await Repository.DeleteOneAsync(id, isActualDelete: true);

        public IReadOnlyList<FamilyMember> BuildDefaultMembers() =>
        [
            new() { Id = "member-kai", Name = "凯", Role = "家庭管理员", AccentColor = "#245b90", CreateTime = DateTime.UtcNow, UpdateTime = DateTime.UtcNow },
            new() { Id = "member-ling", Name = "玲", Role = "共同记账成员", AccentColor = "#dc6e2f", CreateTime = DateTime.UtcNow, UpdateTime = DateTime.UtcNow },
            new() { Id = "member-public", Name = "家庭公共", Role = "共享支出归口", AccentColor = "#2f7d72", CreateTime = DateTime.UtcNow, UpdateTime = DateTime.UtcNow }
        ];
    }
}
