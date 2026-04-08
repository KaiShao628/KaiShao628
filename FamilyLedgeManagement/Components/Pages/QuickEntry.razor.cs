using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Models;
using FamilyLedgeManagement.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FamilyLedgeManagement.Components.Pages;

public partial class QuickEntry
{
    [Inject]
    private LedgerService LedgerService { get; set; } = default!;

    protected QuickEntryContextDto? Context { get; set; }
    protected QuickEntryRequestDto Request { get; set; } = new();
    protected string? Message { get; set; }

    protected string KindValue
    {
        get => Request.Kind.ToString();
        set
        {
            if (Enum.TryParse<TransactionKind>(value, out var kind))
            {
                Request.Kind = kind;
                if (Context is not null && FilteredCategories.All(x => x.Id != Request.CategoryId))
                {
                    Request.CategoryId = FilteredCategories.First().Id;
                }
            }
        }
    }

    protected IEnumerable<LedgerCategoryDto> FilteredCategories =>
        Context?.Categories.Where(x => x.Kind == Request.Kind) ?? Enumerable.Empty<LedgerCategoryDto>();

    protected override async Task OnInitializedAsync()
    {
        Context = await LedgerService.GetQuickEntryContextAsync();
        ResetRequest();
    }

    protected async Task SaveAsync(EditContext _)
    {
        var transaction = await LedgerService.AddTransactionAsync(Request);
        Message = $"已记录 {transaction.CategoryName} {transaction.Amount:C}。";
        ResetRequest();
    }

    protected void FillDinnerPreset()
    {
        if (Context is null) return;
        Request.Kind = TransactionKind.Expense;
        Request.CategoryId = Context.Categories.First(x => x.Name == "餐饮").Id;
        Request.MerchantName = "晚餐";
        Request.Note = "可直接改成具体餐厅";
        Request.OccurredAt = DateTimeOffset.Now;
    }

    protected void FillGroceriesPreset()
    {
        if (Context is null) return;
        Request.Kind = TransactionKind.Expense;
        Request.CategoryId = Context.Categories.First(x => x.Name == "买菜").Id;
        Request.MerchantName = "超市/生鲜";
        Request.Note = "日常采购";
        Request.OccurredAt = DateTimeOffset.Now;
    }

    private void ResetRequest()
    {
        if (Context is null) return;
        var defaultCategory = Context.Categories.First(x => x.Kind == TransactionKind.Expense);
        Request = new QuickEntryRequestDto
        {
            Kind = TransactionKind.Expense,
            CategoryId = defaultCategory.Id,
            MemberId = Context.Members.First().Id,
            PaymentMethod = Context.PaymentMethods.First(),
            Amount = 0,
            MerchantName = string.Empty,
            Note = string.Empty,
            OccurredAt = DateTimeOffset.Now
        };
    }
}

