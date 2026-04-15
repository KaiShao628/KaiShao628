using FamilyLedgeManagement.IRepositories.ICaptureDraftRepositories;
using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Services
{
    /// <summary>
    /// 截图草稿数据库业务服务。
    /// </summary>
    public class CaptureDraftService : ServiceBase<ICaptureDraftRepository>
    {
        public async Task<List<CaptureDraft>> GetAllAsync()
            => (await Repository.GetEntityListAsync(false))
                .OrderByDescending(x => x.CapturedAt)
                .ToList();

        public async Task<CaptureDraft?> GetByIdAsync(string id)
        {
            var drafts = await Repository.GetEntityListByExpressionAsync(x => !x.IsDeleted && x.Id == id);
            return drafts.FirstOrDefault();
        }

        public async Task<string> AddAsync(CaptureDraft entity)
        {
            entity.CreateTime = DateTime.UtcNow;
            entity.UpdateTime = DateTime.UtcNow;
            entity.IsDeleted = false;
            return await Repository.AddOneAsync(entity);
        }

        public async Task<bool> UpdateAsync(CaptureDraft entity)
        {
            var current = await GetByIdAsync(entity.Id);
            if (current is null)
            {
                return false;
            }

            current.MemberId = entity.MemberId;
            current.SuggestedCategoryId = entity.SuggestedCategoryId;
            current.SuggestedAmount = entity.SuggestedAmount;
            current.MerchantName = entity.MerchantName;
            current.ProductName = entity.ProductName;
            current.PaymentMethod = entity.PaymentMethod;
            current.Source = entity.Source;
            current.RecognizedText = entity.RecognizedText;
            current.ImageUrl = entity.ImageUrl;
            current.OriginalFileName = entity.OriginalFileName;
            current.RecognizedOccurredAt = entity.RecognizedOccurredAt;
            current.CapturedAt = entity.CapturedAt;
            current.Status = entity.Status;
            current.UpdateTime = DateTime.UtcNow;

            await Repository.DeleteOneAsync(current.Id, isActualDelete: true);
            return !string.IsNullOrWhiteSpace(await Repository.AddOneAsync(current));
        }

        public async Task<bool> DeleteAsync(string id)
            => await Repository.DeleteOneAsync(id, isActualDelete: true);
    }
}
