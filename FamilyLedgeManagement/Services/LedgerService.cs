using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Models;
using FamilyLedgeManagement.Repositories;

namespace FamilyLedgeManagement.Services;

public sealed class LedgerService
{
    private static readonly IReadOnlyList<string> DefaultPaymentMethods =
    [
        "微信支付",
        "支付宝",
        "银行卡",
        "现金",
        "家庭公共账户"
    ];

    private readonly ILedgerRepository _repository;
    private readonly TimeProvider _timeProvider;

    public LedgerService(ILedgerRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task EnsureSeedDataAsync(CancellationToken cancellationToken = default)
    {
        if (!(await _repository.GetMembersAsync(cancellationToken)).Any())
        {
            await _repository.SaveMembersAsync(BuildDefaultMembers(), cancellationToken);
        }

        if (!(await _repository.GetCategoriesAsync(cancellationToken)).Any())
        {
            await _repository.SaveCategoriesAsync(BuildDefaultCategories(), cancellationToken);
        }

        if (!(await _repository.GetTransactionsAsync(cancellationToken)).Any())
        {
            foreach (var transaction in BuildDefaultTransactions())
            {
                await _repository.AddTransactionAsync(transaction, cancellationToken);
            }
        }

        if (!(await _repository.GetCaptureDraftsAsync(cancellationToken)).Any())
        {
            foreach (var draft in BuildDefaultCaptureDrafts())
            {
                await _repository.AddCaptureDraftAsync(draft, cancellationToken);
            }
        }
    }

    public async Task<DashboardSnapshotDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var members = await _repository.GetMembersAsync(cancellationToken);
        var categories = await _repository.GetCategoriesAsync(cancellationToken);
        var transactions = await _repository.GetTransactionsAsync(cancellationToken);
        var drafts = await _repository.GetCaptureDraftsAsync(cancellationToken);

        var now = _timeProvider.GetLocalNow();
        var monthTransactions = transactions.Where(x => x.OccurredAt.Year == now.Year && x.OccurredAt.Month == now.Month).ToList();
        var monthExpense = monthTransactions.Where(x => x.Kind == TransactionKind.Expense).Sum(x => x.Amount);
        var monthIncome = monthTransactions.Where(x => x.Kind == TransactionKind.Income).Sum(x => x.Amount);

        return new DashboardSnapshotDto
        {
            MonthExpense = monthExpense,
            MonthIncome = monthIncome,
            NetBalance = monthIncome - monthExpense,
            PendingCaptureCount = drafts.Count(x => x.Status == CaptureDraftStatus.Pending),
            RecentTransactions = transactions.OrderByDescending(x => x.OccurredAt).Take(6).Select(x => MapTransaction(x, members, categories)).ToList()
        };
    }

    public async Task<QuickEntryContextDto> GetQuickEntryContextAsync(CancellationToken cancellationToken = default)
    {
        var members = await _repository.GetMembersAsync(cancellationToken);
        var categories = await _repository.GetCategoriesAsync(cancellationToken);

        return new QuickEntryContextDto
        {
            Members = members.OrderBy(x => x.Name).Select(MapMember).ToList(),
            Categories = categories.OrderBy(x => x.Kind).ThenBy(x => x.Name).Select(MapCategory).ToList(),
            PaymentMethods = DefaultPaymentMethods
        };
    }

    public async Task<IReadOnlyList<TransactionListItemDto>> GetTransactionsAsync(DateOnly month, CancellationToken cancellationToken = default)
    {
        var members = await _repository.GetMembersAsync(cancellationToken);
        var categories = await _repository.GetCategoriesAsync(cancellationToken);
        var transactions = await _repository.GetTransactionsAsync(cancellationToken);

        return transactions
            .Where(x => x.OccurredAt.Year == month.Year && x.OccurredAt.Month == month.Month)
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => MapTransaction(x, members, categories))
            .ToList();
    }

    public async Task<IReadOnlyList<CaptureDraftListItemDto>> GetCaptureDraftsAsync(CancellationToken cancellationToken = default)
    {
        var members = await _repository.GetMembersAsync(cancellationToken);
        var categories = await _repository.GetCategoriesAsync(cancellationToken);
        var drafts = await _repository.GetCaptureDraftsAsync(cancellationToken);

        return drafts.OrderByDescending(x => x.CapturedAt).Select(x => MapDraft(x, members, categories)).ToList();
    }

    public async Task<TransactionListItemDto> AddTransactionAsync(QuickEntryRequestDto request, CancellationToken cancellationToken = default)
    {
        var members = await _repository.GetMembersAsync(cancellationToken);
        var categories = await _repository.GetCategoriesAsync(cancellationToken);
        var category = categories.First(x => x.Id == request.CategoryId);

        var transaction = new LedgerTransaction
        {
            Id = Guid.NewGuid().ToString("N"),
            Kind = request.Kind,
            MemberId = request.MemberId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            MerchantName = request.MerchantName.Trim(),
            PaymentMethod = request.PaymentMethod.Trim(),
            Note = request.Note.Trim(),
            OccurredAt = request.OccurredAt ?? _timeProvider.GetLocalNow(),
            CreatedAt = _timeProvider.GetLocalNow()
        };

        if (category.Kind != transaction.Kind)
        {
            throw new InvalidOperationException("分类与收支类型不匹配。");
        }

        await _repository.AddTransactionAsync(transaction, cancellationToken);
        return MapTransaction(transaction, members, categories);
    }

    public async Task<CaptureDraftListItemDto> AddCaptureDraftAsync(CaptureDraftRequestDto request, CancellationToken cancellationToken = default)
    {
        var members = await _repository.GetMembersAsync(cancellationToken);
        var categories = await _repository.GetCategoriesAsync(cancellationToken);

        var memberId = string.IsNullOrWhiteSpace(request.MemberId) ? members.First().Id : request.MemberId;
        var categoryId = string.IsNullOrWhiteSpace(request.SuggestedCategoryId)
            ? categories.First(x => x.Kind == TransactionKind.Expense).Id
            : request.SuggestedCategoryId;

        var draft = new CaptureDraft
        {
            Id = Guid.NewGuid().ToString("N"),
            MemberId = memberId,
            SuggestedCategoryId = categoryId,
            SuggestedAmount = request.SuggestedAmount,
            MerchantName = request.MerchantName.Trim(),
            Source = request.Source.Trim(),
            RecognizedText = request.RecognizedText.Trim(),
            CapturedAt = request.CapturedAt ?? _timeProvider.GetLocalNow(),
            Status = CaptureDraftStatus.Pending
        };

        await _repository.AddCaptureDraftAsync(draft, cancellationToken);
        return MapDraft(draft, members, categories);
    }

    public async Task PromoteCaptureDraftAsync(string draftId, CancellationToken cancellationToken = default)
    {
        var draft = await _repository.GetCaptureDraftAsync(draftId, cancellationToken) ?? throw new InvalidOperationException("未找到对应的截图草稿。");

        if (draft.Status != CaptureDraftStatus.Pending || draft.SuggestedAmount is null)
        {
            return;
        }

        await AddTransactionAsync(new QuickEntryRequestDto
        {
            Kind = TransactionKind.Expense,
            MemberId = draft.MemberId,
            CategoryId = draft.SuggestedCategoryId,
            Amount = draft.SuggestedAmount.Value,
            MerchantName = draft.MerchantName,
            PaymentMethod = "微信支付",
            Note = $"截图辅助入账：{draft.Source}",
            OccurredAt = draft.CapturedAt
        }, cancellationToken);

        draft.Status = CaptureDraftStatus.Confirmed;
        await _repository.UpdateCaptureDraftAsync(draft, cancellationToken);
    }

    private static FamilyMemberDto MapMember(FamilyMember member) => new()
    {
        Id = member.Id,
        Name = member.Name,
        Role = member.Role
    };

    private static LedgerCategoryDto MapCategory(LedgerCategory category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Kind = category.Kind
    };

    private static TransactionListItemDto MapTransaction(LedgerTransaction transaction, IReadOnlyList<FamilyMember> members, IReadOnlyList<LedgerCategory> categories)
    {
        var memberName = members.FirstOrDefault(x => x.Id == transaction.MemberId)?.Name ?? "未分配成员";
        var categoryName = categories.FirstOrDefault(x => x.Id == transaction.CategoryId)?.Name ?? "未分类";

        return new TransactionListItemDto
        {
            Id = transaction.Id,
            MemberName = memberName,
            CategoryName = categoryName,
            Kind = transaction.Kind,
            Amount = transaction.Amount,
            MerchantName = string.IsNullOrWhiteSpace(transaction.MerchantName) ? "未填写" : transaction.MerchantName,
            PaymentMethod = string.IsNullOrWhiteSpace(transaction.PaymentMethod) ? "未填写" : transaction.PaymentMethod,
            Note = string.IsNullOrWhiteSpace(transaction.Note) ? "-" : transaction.Note,
            OccurredAt = transaction.OccurredAt
        };
    }

    private static CaptureDraftListItemDto MapDraft(CaptureDraft draft, IReadOnlyList<FamilyMember> members, IReadOnlyList<LedgerCategory> categories)
    {
        var memberName = members.FirstOrDefault(x => x.Id == draft.MemberId)?.Name ?? "未指定成员";
        var categoryName = categories.FirstOrDefault(x => x.Id == draft.SuggestedCategoryId)?.Name ?? "待分类";

        return new CaptureDraftListItemDto
        {
            Id = draft.Id,
            MemberName = memberName,
            CategoryName = categoryName,
            SuggestedAmount = draft.SuggestedAmount,
            MerchantName = string.IsNullOrWhiteSpace(draft.MerchantName) ? "待识别商户" : draft.MerchantName,
            Source = draft.Source,
            RecognizedText = draft.RecognizedText,
            CapturedAt = draft.CapturedAt,
            Status = draft.Status
        };
    }

    private static IReadOnlyList<FamilyMember> BuildDefaultMembers() =>
    [
        new() { Id = "member-kai", Name = "凯", Role = "家庭管理员", AccentColor = "#245b90" },
        new() { Id = "member-ling", Name = "玲", Role = "共同记账成员", AccentColor = "#dc6e2f" },
        new() { Id = "member-public", Name = "家庭公共", Role = "共享支出归口", AccentColor = "#2f7d72" }
    ];

    private static IReadOnlyList<LedgerCategory> BuildDefaultCategories() =>
    [
        new() { Id = "cat-food", Name = "餐饮", Kind = TransactionKind.Expense, Color = "#dc6e2f" },
        new() { Id = "cat-grocery", Name = "买菜", Kind = TransactionKind.Expense, Color = "#ef8f3d" },
        new() { Id = "cat-traffic", Name = "交通", Kind = TransactionKind.Expense, Color = "#245b90" },
        new() { Id = "cat-home", Name = "家居日用", Kind = TransactionKind.Expense, Color = "#2f7d72" },
        new() { Id = "cat-entertainment", Name = "娱乐", Kind = TransactionKind.Expense, Color = "#7d4c8f" },
        new() { Id = "cat-salary", Name = "工资", Kind = TransactionKind.Income, Color = "#24745c" },
        new() { Id = "cat-bonus", Name = "红包", Kind = TransactionKind.Income, Color = "#4a9d67" }
    ];

    private IReadOnlyList<LedgerTransaction> BuildDefaultTransactions()
    {
        var now = _timeProvider.GetLocalNow();
        return
        [
            new() { Id = Guid.NewGuid().ToString("N"), MemberId = "member-kai", CategoryId = "cat-food", Kind = TransactionKind.Expense, Amount = 58, MerchantName = "晚餐", PaymentMethod = "微信支付", Note = "工作日晚餐", OccurredAt = now.AddDays(-1), CreatedAt = now.AddDays(-1) },
            new() { Id = Guid.NewGuid().ToString("N"), MemberId = "member-ling", CategoryId = "cat-grocery", Kind = TransactionKind.Expense, Amount = 126.50m, MerchantName = "盒马", PaymentMethod = "支付宝", Note = "周末买菜", OccurredAt = now.AddDays(-2), CreatedAt = now.AddDays(-2) },
            new() { Id = Guid.NewGuid().ToString("N"), MemberId = "member-public", CategoryId = "cat-home", Kind = TransactionKind.Expense, Amount = 89.90m, MerchantName = "日用品补货", PaymentMethod = "银行卡", Note = "厨房清洁和纸品", OccurredAt = now.AddDays(-4), CreatedAt = now.AddDays(-4) },
            new() { Id = Guid.NewGuid().ToString("N"), MemberId = "member-kai", CategoryId = "cat-salary", Kind = TransactionKind.Income, Amount = 5600m, MerchantName = "工资到账", PaymentMethod = "银行卡", Note = "本月工资", OccurredAt = new DateTimeOffset(now.Year, now.Month, 1, 9, 0, 0, now.Offset), CreatedAt = new DateTimeOffset(now.Year, now.Month, 1, 9, 0, 0, now.Offset) }
        ];
    }

    private IReadOnlyList<CaptureDraft> BuildDefaultCaptureDrafts()
    {
        var now = _timeProvider.GetLocalNow();
        return
        [
            new() { Id = Guid.NewGuid().ToString("N"), MemberId = "member-ling", SuggestedCategoryId = "cat-food", SuggestedAmount = 42m, MerchantName = "蜜雪冰城", Source = "iPhone Shortcut", RecognizedText = "微信支付成功 42.00 商户: 蜜雪冰城", CapturedAt = now.AddHours(-5), Status = CaptureDraftStatus.Pending },
            new() { Id = Guid.NewGuid().ToString("N"), MemberId = "member-kai", SuggestedCategoryId = "cat-traffic", SuggestedAmount = 18m, MerchantName = "滴滴出行", Source = "订单截图", RecognizedText = "行程已完成 合计 18.00", CapturedAt = now.AddHours(-12), Status = CaptureDraftStatus.Pending }
        ];
    }
}

