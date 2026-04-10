using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Models;
using FamilyLedgeManagement.Repositories;
using Microsoft.AspNetCore.Hosting;

namespace FamilyLedgeManagement.Services;

/// <summary>
/// 家庭账本应用服务，负责页面 DTO 编排、实体映射、业务校验和仓储调用。
/// </summary>
public sealed class LedgerService
{
    /// <summary>
    /// 截图上传允许的最大文件大小。
    /// </summary>
    private const long MaxCaptureImageSize = 10 * 1024 * 1024;

    /// <summary>
    /// 系统内置常用支付方式。
    /// </summary>
    private static readonly IReadOnlyList<string> DefaultPaymentMethods =
    [
        "微信支付",
        "支付宝",
        "银行卡",
        "现金",
        "家庭公共账户"
    ];

    /// <summary>
    /// 账本数据仓储。
    /// </summary>
    private readonly ILedgerRepository _repository;

    /// <summary>
    /// 系统时间提供器，便于测试和统一取当前时间。
    /// </summary>
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// 截图文件保存目录。
    /// </summary>
    private readonly string _captureImageDirectory;

    /// <summary>
    /// 截图 OCR 结构化识别服务。
    /// </summary>
    private readonly StructuredCaptureRecognitionService _captureRecognitionService;

    /// <summary>
    /// 创建家庭账本应用服务。
    /// </summary>
    public LedgerService(
        ILedgerRepository repository,
        TimeProvider timeProvider,
        IWebHostEnvironment webHostEnvironment,
        StructuredCaptureRecognitionService captureRecognitionService)
    {
        _repository = repository;
        _timeProvider = timeProvider;
        _captureImageDirectory = Path.Combine(webHostEnvironment.ContentRootPath, "uploads", "capture-drafts");
        _captureRecognitionService = captureRecognitionService;
    }

    /// <summary>
    /// 初始化演示成员、分类和账单种子数据。
    /// </summary>
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

    }

    /// <summary>
    /// 获取首页概览数据。
    /// </summary>
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

    /// <summary>
    /// 获取快速记账页面所需的成员、分类和支付方式上下文。
    /// </summary>
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

    /// <summary>
    /// 获取成员列表 DTO。
    /// </summary>
    public async Task<IReadOnlyList<FamilyMemberDto>> GetMembersAsync(CancellationToken cancellationToken = default)
    {
        var members = await _repository.GetMembersAsync(cancellationToken);
        return members.OrderBy(x => x.Name).Select(MapMember).ToList();
    }

    /// <summary>
    /// 获取分类列表 DTO。
    /// </summary>
    public async Task<IReadOnlyList<LedgerCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _repository.GetCategoriesAsync(cancellationToken);
        return categories
            .OrderBy(x => x.Kind)
            .ThenBy(x => x.Name)
            .Select(MapCategory)
            .ToList();
    }

    /// <summary>
    /// 新增或更新家庭成员。
    /// </summary>
    public async Task SaveMemberAsync(FamilyMemberEditorDto request, CancellationToken cancellationToken = default)
    {
        var members = (await _repository.GetMembersAsync(cancellationToken)).ToList();
        var normalizedName = request.Name.Trim();
        var normalizedRole = request.Role.Trim();
        var normalizedColor = request.AccentColor.Trim();

        if (members.Any(x => x.Id != request.Id && string.Equals(x.Name, normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("成员名称已存在，请换一个名称。");
        }

        var existing = members.FirstOrDefault(x => x.Id == request.Id);
        if (existing is null)
        {
            members.Add(new FamilyMember
            {
                Id = string.IsNullOrWhiteSpace(request.Id) ? Guid.NewGuid().ToString("N") : request.Id,
                Name = normalizedName,
                Role = normalizedRole,
                AccentColor = normalizedColor
            });
        }
        else
        {
            existing.Name = normalizedName;
            existing.Role = normalizedRole;
            existing.AccentColor = normalizedColor;
        }

        await _repository.SaveMembersAsync(members, cancellationToken);
    }

    /// <summary>
    /// 删除家庭成员。
    /// </summary>
    public async Task DeleteMemberAsync(string id, CancellationToken cancellationToken = default)
    {
        var members = (await _repository.GetMembersAsync(cancellationToken)).ToList();
        if (members.RemoveAll(x => x.Id == id) == 0)
        {
            return;
        }

        await _repository.SaveMembersAsync(members, cancellationToken);
    }

    /// <summary>
    /// 新增或更新账单分类。
    /// </summary>
    public async Task SaveCategoryAsync(LedgerCategoryEditorDto request, CancellationToken cancellationToken = default)
    {
        var categories = (await _repository.GetCategoriesAsync(cancellationToken)).ToList();
        var normalizedName = request.Name.Trim();
        var normalizedColor = request.Color.Trim();

        if (categories.Any(x => x.Id != request.Id && x.Kind == request.Kind && string.Equals(x.Name, normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("同类型下已存在同名分类。");
        }

        var existing = categories.FirstOrDefault(x => x.Id == request.Id);
        if (existing is null)
        {
            categories.Add(new LedgerCategory
            {
                Id = string.IsNullOrWhiteSpace(request.Id) ? Guid.NewGuid().ToString("N") : request.Id,
                Name = normalizedName,
                Kind = request.Kind,
                Color = normalizedColor
            });
        }
        else
        {
            existing.Name = normalizedName;
            existing.Kind = request.Kind;
            existing.Color = normalizedColor;
        }

        await _repository.SaveCategoriesAsync(categories, cancellationToken);
    }

    /// <summary>
    /// 删除账单分类。
    /// </summary>
    public async Task DeleteCategoryAsync(string id, CancellationToken cancellationToken = default)
    {
        var categories = (await _repository.GetCategoriesAsync(cancellationToken)).ToList();
        if (categories.RemoveAll(x => x.Id == id) == 0)
        {
            return;
        }

        await _repository.SaveCategoriesAsync(categories, cancellationToken);
    }

    /// <summary>
    /// 按月份获取账单流水列表。
    /// </summary>
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

    /// <summary>
    /// 获取截图草稿列表，当前仅用于兼容历史草稿数据。
    /// </summary>
    public async Task<IReadOnlyList<CaptureDraftListItemDto>> GetCaptureDraftsAsync(CancellationToken cancellationToken = default)
    {
        var members = await _repository.GetMembersAsync(cancellationToken);
        var categories = await _repository.GetCategoriesAsync(cancellationToken);
        var drafts = await _repository.GetCaptureDraftsAsync(cancellationToken);

        return drafts.OrderByDescending(x => x.CapturedAt).Select(x => MapDraft(x, members, categories)).ToList();
    }

    /// <summary>
    /// 快速新增一条正式账单。
    /// </summary>
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

    /// <summary>
    /// 新增或更新一条正式账单。
    /// </summary>
    public async Task SaveTransactionAsync(TransactionEditorDto request, CancellationToken cancellationToken = default)
    {
        var members = await _repository.GetMembersAsync(cancellationToken);
        var categories = await _repository.GetCategoriesAsync(cancellationToken);
        var category = categories.First(x => x.Id == request.CategoryId);

        if (category.Kind != request.Kind)
        {
            throw new InvalidOperationException("分类与收支类型不匹配。");
        }

        if (string.IsNullOrWhiteSpace(request.Id))
        {
            await AddTransactionAsync(new QuickEntryRequestDto
            {
                Kind = request.Kind,
                MemberId = request.MemberId,
                CategoryId = request.CategoryId,
                Amount = request.Amount,
                MerchantName = request.MerchantName,
                PaymentMethod = request.PaymentMethod,
                Note = request.Note,
                OccurredAt = request.OccurredAt
            }, cancellationToken);
            return;
        }

        var existing = await _repository.GetTransactionAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException("未找到要编辑的账单。");

        existing.MemberId = request.MemberId;
        existing.CategoryId = request.CategoryId;
        existing.Kind = request.Kind;
        existing.Amount = request.Amount;
        existing.MerchantName = request.MerchantName.Trim();
        existing.PaymentMethod = request.PaymentMethod.Trim();
        existing.Note = request.Note.Trim();
        existing.OccurredAt = request.OccurredAt ?? existing.OccurredAt;

        await _repository.UpdateTransactionAsync(existing, cancellationToken);
    }

    /// <summary>
    /// 删除正式账单。
    /// </summary>
    public Task DeleteTransactionAsync(string id, CancellationToken cancellationToken = default)
        => _repository.DeleteTransactionAsync(id, cancellationToken);

    /// <summary>
    /// 新增截图草稿，保留给历史接口兼容。
    /// </summary>
    public async Task<CaptureDraftListItemDto> AddCaptureDraftAsync(CaptureDraftRequestDto request, CancellationToken cancellationToken = default)
        => await AddCaptureDraftCoreAsync(request, null, null, cancellationToken);

    /// <summary>
    /// 保存截图文件并新增截图草稿，保留给历史接口兼容。
    /// </summary>
    public async Task<CaptureDraftListItemDto> AddCaptureDraftWithImageAsync(
        CaptureDraftRequestDto request,
        Stream imageStream,
        string originalFileName,
        CancellationToken cancellationToken = default)
        => await AddCaptureDraftCoreAsync(request, imageStream, originalFileName, cancellationToken);

    /// <summary>
    /// 对截图进行预识别，用于页面选择图片后回填识别结果。
    /// </summary>
    public async Task<CaptureRecognitionPreviewDto> PreviewCaptureRecognitionAsync(
        Stream imageStream,
        string originalFileName,
        string existingText,
        decimal? existingAmount,
        string existingMerchant,
        CancellationToken cancellationToken = default)
    {
        var tempDirectory = Path.Combine(_captureImageDirectory, "preview");
        Directory.CreateDirectory(tempDirectory);

        var extension = Path.GetExtension(originalFileName);
        var normalizedExtension = string.IsNullOrWhiteSpace(extension) ? ".png" : extension.ToLowerInvariant();
        var tempFilePath = Path.Combine(tempDirectory, $"{Guid.NewGuid():N}{normalizedExtension}");

        await using (var targetStream = File.Create(tempFilePath))
        {
            await imageStream.CopyToAsync(targetStream, cancellationToken);
        }

        try
        {
            var result = await _captureRecognitionService.AnalyzeAsync(
                tempFilePath,
                existingText,
                existingAmount,
                existingMerchant,
                cancellationToken);

            return new CaptureRecognitionPreviewDto
            {
                SuggestedAmount = result.SuggestedAmount,
                MerchantName = result.MerchantName,
                ProductName = result.ProductName,
                PaymentMethod = result.PaymentMethod,
                RecognizedText = result.RecognizedText,
                OccurredAt = result.OccurredAt
            };
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    /// <summary>
    /// 从截图识别结果直接创建正式支出账单。
    /// </summary>
    public async Task<TransactionListItemDto> AddTransactionFromCaptureAsync(
        CaptureDraftRequestDto request,
        Stream imageStream,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        var recognition = await AnalyzeCaptureAsync(
            imageStream,
            originalFileName,
            request.RecognizedText,
            request.SuggestedAmount,
            request.MerchantName,
            cancellationToken);

        var amount = recognition.SuggestedAmount ?? request.SuggestedAmount;
        if (amount is null || amount <= 0)
        {
            throw new InvalidOperationException("截图识别后仍未拿到有效金额，暂时不能自动入账。");
        }

        var merchantOrProduct = !string.IsNullOrWhiteSpace(recognition.ProductName)
            ? recognition.ProductName
            : recognition.MerchantName;

        return await AddTransactionAsync(new QuickEntryRequestDto
        {
            Kind = TransactionKind.Expense,
            MemberId = request.MemberId,
            CategoryId = request.SuggestedCategoryId,
            Amount = amount.Value,
            MerchantName = merchantOrProduct,
            PaymentMethod = string.IsNullOrWhiteSpace(recognition.PaymentMethod) ? "微信支付" : recognition.PaymentMethod,
            Note = BuildCaptureNote(request.Source, recognition.MerchantName, recognition.ProductName),
            OccurredAt = recognition.OccurredAt ?? request.CapturedAt ?? _timeProvider.GetLocalNow()
        }, cancellationToken);
    }

    /// <summary>
    /// 将历史截图草稿转成正式支出账单。
    /// </summary>
    public async Task PromoteCaptureDraftAsync(string draftId, CancellationToken cancellationToken = default)
    {
        var draft = await _repository.GetCaptureDraftAsync(draftId, cancellationToken) ?? throw new InvalidOperationException("未找到对应的截图草稿。");

        if (draft.Status != CaptureDraftStatus.Pending)
        {
            throw new InvalidOperationException("这条截图草稿已经处理过了。");
        }

        if (draft.SuggestedAmount is null || draft.SuggestedAmount <= 0)
        {
            throw new InvalidOperationException("这条截图草稿还没有可用金额，暂时不能转为正式支出。请先补金额后再入账。");
        }

        await AddTransactionAsync(new QuickEntryRequestDto
        {
            Kind = TransactionKind.Expense,
            MemberId = draft.MemberId,
            CategoryId = draft.SuggestedCategoryId,
            Amount = draft.SuggestedAmount.Value,
            MerchantName = string.IsNullOrWhiteSpace(draft.ProductName) ? draft.MerchantName : draft.ProductName,
            PaymentMethod = string.IsNullOrWhiteSpace(draft.PaymentMethod) ? "微信支付" : draft.PaymentMethod,
            Note = $"截图辅助入账：{draft.Source}",
            OccurredAt = draft.RecognizedOccurredAt ?? draft.CapturedAt
        }, cancellationToken);

        draft.Status = CaptureDraftStatus.Confirmed;
        await _repository.UpdateCaptureDraftAsync(draft, cancellationToken);
    }

    /// <summary>
    /// 新增截图草稿的核心实现。
    /// </summary>
    private async Task<CaptureDraftListItemDto> AddCaptureDraftCoreAsync(
        CaptureDraftRequestDto request,
        Stream? imageStream,
        string? originalFileName,
        CancellationToken cancellationToken)
    {
        var members = await _repository.GetMembersAsync(cancellationToken);
        var categories = await _repository.GetCategoriesAsync(cancellationToken);

        var memberId = string.IsNullOrWhiteSpace(request.MemberId) ? members.First().Id : request.MemberId;
        var categoryId = string.IsNullOrWhiteSpace(request.SuggestedCategoryId)
            ? categories.First(x => x.Kind == TransactionKind.Expense).Id
            : request.SuggestedCategoryId;

        var imageUrl = string.Empty;
        var safeOriginalFileName = string.Empty;

        if (imageStream is not null && !string.IsNullOrWhiteSpace(originalFileName))
        {
            (imageUrl, safeOriginalFileName) = await SaveCaptureImageAsync(imageStream, originalFileName, cancellationToken);
        }

        var recognition = string.IsNullOrWhiteSpace(imageUrl)
            ? new StructuredCaptureRecognitionResult
            {
                RecognizedText = request.RecognizedText.Trim(),
                SuggestedAmount = request.SuggestedAmount,
                MerchantName = request.MerchantName.Trim()
            }
            : await _captureRecognitionService.AnalyzeAsync(
                Path.Combine(_captureImageDirectory, Path.GetFileName(imageUrl)),
                request.RecognizedText.Trim(),
                request.SuggestedAmount,
                request.MerchantName.Trim(),
                cancellationToken);

        var draft = new CaptureDraft
        {
            Id = Guid.NewGuid().ToString("N"),
            MemberId = memberId,
            SuggestedCategoryId = categoryId,
            SuggestedAmount = recognition.SuggestedAmount,
            MerchantName = recognition.MerchantName,
            ProductName = recognition.ProductName,
            PaymentMethod = recognition.PaymentMethod,
            Source = request.Source.Trim(),
            RecognizedText = recognition.RecognizedText,
            ImageUrl = imageUrl,
            OriginalFileName = safeOriginalFileName,
            RecognizedOccurredAt = recognition.OccurredAt,
            CapturedAt = request.CapturedAt ?? _timeProvider.GetLocalNow(),
            Status = CaptureDraftStatus.Pending
        };

        await _repository.AddCaptureDraftAsync(draft, cancellationToken);
        return MapDraft(draft, members, categories);
    }

    /// <summary>
    /// 临时保存截图并调用 OCR 结构化识别。
    /// </summary>
    private async Task<StructuredCaptureRecognitionResult> AnalyzeCaptureAsync(
        Stream imageStream,
        string originalFileName,
        string existingText,
        decimal? existingAmount,
        string existingMerchant,
        CancellationToken cancellationToken)
    {
        var tempDirectory = Path.Combine(_captureImageDirectory, "preview");
        Directory.CreateDirectory(tempDirectory);

        var extension = Path.GetExtension(originalFileName);
        var normalizedExtension = string.IsNullOrWhiteSpace(extension) ? ".png" : extension.ToLowerInvariant();
        var tempFilePath = Path.Combine(tempDirectory, $"{Guid.NewGuid():N}{normalizedExtension}");

        await using (var targetStream = File.Create(tempFilePath))
        {
            await imageStream.CopyToAsync(targetStream, cancellationToken);
        }

        try
        {
            return await _captureRecognitionService.AnalyzeAsync(
                tempFilePath,
                existingText,
                existingAmount,
                existingMerchant,
                cancellationToken);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    /// <summary>
    /// 根据截图来源、商户和商品信息生成账单备注。
    /// </summary>
    private static string BuildCaptureNote(string source, string merchantName, string productName)
    {
        var parts = new List<string> { $"截图识别：{source}" };

        if (!string.IsNullOrWhiteSpace(merchantName))
        {
            parts.Add($"商户 {merchantName}");
        }

        if (!string.IsNullOrWhiteSpace(productName))
        {
            parts.Add($"商品 {productName}");
        }

        return string.Join("；", parts);
    }

    /// <summary>
    /// 将成员实体映射为展示 DTO。
    /// </summary>
    private static FamilyMemberDto MapMember(FamilyMember member) => new()
    {
        Id = member.Id,
        Name = member.Name,
        Role = member.Role,
        AccentColor = member.AccentColor
    };

    /// <summary>
    /// 将分类实体映射为展示 DTO。
    /// </summary>
    private static LedgerCategoryDto MapCategory(LedgerCategory category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Kind = category.Kind,
        Color = category.Color
    };

    /// <summary>
    /// 将账单实体映射为列表 DTO。
    /// </summary>
    private static TransactionListItemDto MapTransaction(LedgerTransaction transaction, IReadOnlyList<FamilyMember> members, IReadOnlyList<LedgerCategory> categories)
    {
        var memberName = members.FirstOrDefault(x => x.Id == transaction.MemberId)?.Name ?? "未分配成员";
        var categoryName = categories.FirstOrDefault(x => x.Id == transaction.CategoryId)?.Name ?? "未分类";

        return new TransactionListItemDto
        {
            Id = transaction.Id,
            MemberId = transaction.MemberId,
            CategoryId = transaction.CategoryId,
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

    /// <summary>
    /// 将截图草稿实体映射为列表 DTO。
    /// </summary>
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
            ProductName = draft.ProductName,
            PaymentMethod = draft.PaymentMethod,
            Source = draft.Source,
            RecognizedText = draft.RecognizedText,
            ImageUrl = draft.ImageUrl,
            OriginalFileName = draft.OriginalFileName,
            RecognizedOccurredAt = draft.RecognizedOccurredAt,
            CapturedAt = draft.CapturedAt,
            Status = draft.Status
        };
    }

    /// <summary>
    /// 构建默认家庭成员。
    /// </summary>
    private static IReadOnlyList<FamilyMember> BuildDefaultMembers() =>
    [
        new() { Id = "member-kai", Name = "凯", Role = "家庭管理员", AccentColor = "#245b90" },
        new() { Id = "member-ling", Name = "玲", Role = "共同记账成员", AccentColor = "#dc6e2f" },
        new() { Id = "member-public", Name = "家庭公共", Role = "共享支出归口", AccentColor = "#2f7d72" }
    ];

    /// <summary>
    /// 构建默认账单分类。
    /// </summary>
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

    /// <summary>
    /// 构建默认账单流水。
    /// </summary>
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

    /// <summary>
    /// 保存截图图片到本地上传目录。
    /// </summary>
    private async Task<(string ImageUrl, string OriginalFileName)> SaveCaptureImageAsync(Stream imageStream, string originalFileName, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_captureImageDirectory);

        var extension = Path.GetExtension(originalFileName);
        var normalizedExtension = string.IsNullOrWhiteSpace(extension) ? ".png" : extension.ToLowerInvariant();
        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".webp", ".heic" };

        if (!allowedExtensions.Contains(normalizedExtension))
        {
            throw new InvalidOperationException("仅支持 png、jpg、jpeg、webp、heic 图片。");
        }

        var storedFileName = $"{Guid.NewGuid():N}{normalizedExtension}";
        var fullPath = Path.Combine(_captureImageDirectory, storedFileName);

        await using var targetStream = File.Create(fullPath);
        await imageStream.CopyToAsync(targetStream, cancellationToken);

        if (targetStream.Length > MaxCaptureImageSize)
        {
            targetStream.Close();
            File.Delete(fullPath);
            throw new InvalidOperationException("截图图片不能超过 10 MB。");
        }

        return ($"/uploads/capture-drafts/{storedFileName}", Path.GetFileName(originalFileName));
    }
}
