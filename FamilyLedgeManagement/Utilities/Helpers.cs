using BootstrapBlazor.Components;
using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Dtos.DictionaryDtos;
using FamilyLedgeManagement.Models;
using FamilyLedgeManagement.Models.DictionaryModels;

namespace FamilyLedgeManagement.Utilities
{
    public static class Helpers
    {
        public static IEnumerable<int> PageItemSource => new int[] { 50 };

        public static long MaxFileLength => 200 * 1024 * 1024;

        public static IEnumerable<TreeViewItem<DictionaryDto>> CascadingDictionaryTree(IEnumerable<DictionaryDto> items) => items.Select(x => new TreeViewItem<DictionaryDto>(x));


        public static TransactionListItemDto MapTransaction(
            LedgerTransaction transaction,
            Dictionary<string, FamilyMember> membersDic,
            Dictionary<string, LedgerCategory> categoriesDic,
            Dictionary<string, DictionaryValueEntity> dictionaryValueDic)
        {
            var memberName = membersDic.TryGetValue(transaction.MemberId, out var member) ? member.Name : "未分配成员";
            var categoryName = categoriesDic.TryGetValue(transaction.CategoryId, out var category) ? category.Name : "未分类";
            var dictionaryValue = dictionaryValueDic.TryGetValue(transaction.Kind, out var dictValue) ? dictValue.Value : "未知类型";

            return new TransactionListItemDto
            {
                Id = transaction.Id,
                MemberId = transaction.MemberId,
                CategoryId = transaction.CategoryId,
                MemberName = memberName,
                CategoryName = categoryName,
                Kind = transaction.Kind,
                KindName = dictionaryValue,
                Amount = transaction.Amount,
                MerchantName = string.IsNullOrWhiteSpace(transaction.MerchantName) ? "未填写" : transaction.MerchantName,
                PaymentMethod = string.IsNullOrWhiteSpace(transaction.PaymentMethod) ? "未填写" : transaction.PaymentMethod,
                Note = string.IsNullOrWhiteSpace(transaction.Note) ? "-" : transaction.Note,
                OccurredAt = transaction.OccurredAt
            };
        }
    }
}
