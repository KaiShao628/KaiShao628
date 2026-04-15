using FamilyLedgeManagement.Database;
using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Enums;
using FamilyLedgeManagement.IRepositories.ICaptureDraftRepositories;
using FamilyLedgeManagement.IRepositories.IFamilyMemberRepositories;
using FamilyLedgeManagement.IRepositories.ILedgerCategoryRepositories;
using FamilyLedgeManagement.IRepositories.ITransactionRepositories;
using FamilyLedgeManagement.Models;
using FamilyLedgeManagement.Repositories.TransactionRepositories;

namespace FamilyLedgeManagement.Services
{
    /// <summary>
    /// 页面与接口使用的聚合业务服务，内部组合各个 DB 层服务。
    /// </summary>
    public class LedgerService
    {
        private readonly ICaptureDraftRepository _captureDraftRepository = FamilyLedgeMongoDBClient.Instance.GetRepository<ICaptureDraftRepository>();
        private readonly IFamilyMemberRepository _familyMemberRepository = FamilyLedgeMongoDBClient.Instance.GetRepository<IFamilyMemberRepository>();
        private readonly ILedgerCategoryRepository _ledgerCategoryRepository = FamilyLedgeMongoDBClient.Instance.GetRepository<ILedgerCategoryRepository>();
        private readonly ITransactionRepository _transactionRepository = FamilyLedgeMongoDBClient.Instance.GetRepository<ITransactionRepository>();

        private readonly FamilyMemberService _familyMemberService;
        private readonly CaptureDraftService _captureDraftService;
        private readonly TransactionService _transactionService;
        private readonly LedgerCategoryService _ledgerCategoryService;

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

        private readonly StructuredCaptureRecognitionService _captureRecognitionService;
        private readonly TimeProvider _timeProvider;
        private readonly string _captureImageDirectory;

        /// <summary>
        /// 构造聚合业务服务。
        /// </summary>
        public LedgerService(FamilyMemberService familyMemberService, CaptureDraftService captureDraftService,
            TransactionService transactionService, LedgerCategoryService ledgerCategoryService, StructuredCaptureRecognitionService captureRecognitionService, TimeProvider timeProvider, IWebHostEnvironment webHostEnvironment)
        {
            _familyMemberService = familyMemberService;
            _captureDraftService = captureDraftService;
            _transactionService = transactionService;
            _ledgerCategoryService = ledgerCategoryService;
            _captureRecognitionService = captureRecognitionService;
            _timeProvider = timeProvider;
            _captureImageDirectory = Path.Combine(webHostEnvironment.ContentRootPath, "uploads", "capture-drafts");
        }

        /// <summary>
        /// 初始化成员、分类和账单种子数据。
        /// </summary>
        //public async Task EnsureSeedDataAsync(CancellationToken cancellationToken = default)
        //{
        //    if (!(await _familyMemberRepository.GetEntityListAsync()).Any())
        //    {
        //        foreach (var member in _familyMemberService.BuildDefaultMembers())
        //        {
        //            await _familyMemberService.AddAsync(member);
        //        }
        //    }

        //    if (!(await _ledgerCategoryService.GetAllAsync()).Any())
        //    {
        //        foreach (var category in _ledgerCategoryService.BuildDefaultCategories())
        //        {
        //            await _ledgerCategoryService.AddAsync(category);
        //        }
        //    }

        //    if (!(await _transactionService.GetAllAsync()).Any())
        //    {
        //        foreach (var transaction in _transactionService.BuildDefaultTransactions(_timeProvider.GetLocalNow()))
        //        {
        //            await _transactionService.AddAsync(transaction);
        //        }
        //    }
        //}

        /// <summary>
        /// 获取首页概览数据。
        /// </summary>
        public async Task<DashboardSnapshotDto> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            var members = await _familyMemberService.GetAllAsync();
            var categories = await _ledgerCategoryService.GetAllAsync();
            var transactions = await _transactionService.GetAllAsync();
            var drafts = await _captureDraftService.GetAllAsync();

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
                RecentTransactions = transactions.Take(6).Select(x => MapTransaction(x, members, categories)).ToList()
            };
        }

        /// <summary>
        /// 获取快速记账上下文。
        /// </summary>
        public async Task<QuickEntryContextDto> GetQuickEntryContextAsync(CancellationToken cancellationToken = default)
        {
            var members = await _familyMemberService.GetAllAsync();
            var categories = await _ledgerCategoryService.GetAllAsync();

            return new QuickEntryContextDto
            {
                Members = members.Select(MapMember).ToList(),
                Categories = categories.Select(MapCategory).ToList(),
                PaymentMethods = DefaultPaymentMethods
            };
        }

        /// <summary>
        /// 获取成员列表 DTO。
        /// </summary>
        public async Task<IReadOnlyList<FamilyMemberDto>> GetMembersAsync(CancellationToken cancellationToken = default)
            => (await _familyMemberService.GetAllAsync()).Select(MapMember).ToList();

        /// <summary>
        /// 获取分类列表 DTO。
        /// </summary>
        public async Task<IReadOnlyList<LedgerCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
            => (await _ledgerCategoryService.GetAllAsync()).Select(MapCategory).ToList();

        /// <summary>
        /// 新增或更新成员。
        /// </summary>
        public async Task SaveMemberAsync(FamilyMemberEditorDto request, CancellationToken cancellationToken = default)
        {
            var normalizedName = request.Name.Trim();
            var normalizedRole = request.Role.Trim();
            var normalizedColor = request.AccentColor.Trim();

            if (await _familyMemberService.CheckNameExistsAsync(normalizedName, request.Id))
            {
                throw new InvalidOperationException("成员名称已存在，请换一个名称。");
            }

            var entity = new FamilyMember
            {
                Id = string.IsNullOrWhiteSpace(request.Id) ? Guid.NewGuid().ToString("N") : request.Id,
                Name = normalizedName,
                Role = normalizedRole,
                AccentColor = normalizedColor
            };

            if (string.IsNullOrWhiteSpace(request.Id))
            {
                await _familyMemberService.AddAsync(entity);
            }
            else
            {
                await _familyMemberService.UpdateAsync(entity);
            }
        }

        /// <summary>
        /// 删除成员。
        /// </summary>
        public async Task DeleteMemberAsync(string id, CancellationToken cancellationToken = default)
            => await _familyMemberService.DeleteAsync(id);

        /// <summary>
        /// 新增或更新分类。
        /// </summary>
        public async Task SaveCategoryAsync(LedgerCategoryEditorDto request, CancellationToken cancellationToken = default)
        {
            var normalizedName = request.Name.Trim();
            var normalizedColor = request.Color.Trim();

            if (await _ledgerCategoryService.CheckNameExistsAsync(normalizedName, request.Kind, request.Id))
            {
                throw new InvalidOperationException("同类型下已存在同名分类。");
            }

            var entity = new LedgerCategory
            {
                Id = string.IsNullOrWhiteSpace(request.Id) ? Guid.NewGuid().ToString("N") : request.Id,
                Name = normalizedName,
                Kind = request.Kind,
                Color = normalizedColor
            };

            if (string.IsNullOrWhiteSpace(request.Id))
            {
                await _ledgerCategoryService.AddAsync(entity);
            }
            else
            {
                await _ledgerCategoryService.UpdateAsync(entity);
            }
        }

        /// <summary>
        /// 删除分类。
        /// </summary>
        public async Task DeleteCategoryAsync(string id, CancellationToken cancellationToken = default)
            => await _ledgerCategoryService.DeleteAsync(id);

        /// <summary>
        /// 按月份获取账单列表。
        /// </summary>
        public async Task<IReadOnlyList<TransactionListItemDto>> GetTransactionsAsync(DateOnly month, CancellationToken cancellationToken = default)
        {
            var members = await _familyMemberService.GetAllAsync();
            var categories = await _ledgerCategoryService.GetAllAsync();
            var transactions = await _transactionService.GetByMonthAsync(month.Year, month.Month);
            return transactions.Select(x => MapTransaction(x, members, categories)).ToList();
        }

        /// <summary>
        /// 获取截图草稿列表。
        /// </summary>
        public async Task<IReadOnlyList<CaptureDraftListItemDto>> GetCaptureDraftsAsync(CancellationToken cancellationToken = default)
        {
            var members = await _familyMemberService.GetAllAsync();
            var categories = await _ledgerCategoryService.GetAllAsync();
            var drafts = await _captureDraftService.GetAllAsync();
            return drafts.Select(x => MapDraft(x, members, categories)).ToList();
        }

        /// <summary>
        /// 新增正式账单。
        /// </summary>
        public async Task<TransactionListItemDto> AddTransactionAsync(QuickEntryRequestDto request, CancellationToken cancellationToken = default)
        {
            var members = await _familyMemberService.GetAllAsync();
            var categories = await _ledgerCategoryService.GetAllAsync();
            var category = categories.First(x => x.Id == request.CategoryId);

            if (category.Kind != request.Kind)
            {
                throw new InvalidOperationException("分类与收支类型不匹配。");
            }

            var transaction = new LedgerTransaction
            {
                Id = Guid.NewGuid().ToString("N"),
                MemberId = request.MemberId,
                CategoryId = request.CategoryId,
                Kind = request.Kind,
                Amount = request.Amount,
                MerchantName = request.MerchantName.Trim(),
                PaymentMethod = request.PaymentMethod.Trim(),
                Note = request.Note.Trim(),
                OccurredAt = request.OccurredAt ?? _timeProvider.GetLocalNow(),
                CreatedAt = _timeProvider.GetLocalNow()
            };

            var id = await _transactionService.AddAsync(transaction);
            transaction.Id = id;
            return MapTransaction(transaction, members, categories);
        }

        /// <summary>
        /// 新增或更新正式账单。
        /// </summary>
        public async Task SaveTransactionAsync(TransactionEditorDto request, CancellationToken cancellationToken = default)
        {
            var categories = await _ledgerCategoryService.GetAllAsync();
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

            var current = await _transactionService.GetByIdAsync(request.Id)
                ?? throw new InvalidOperationException("未找到要编辑的账单。");

            current.MemberId = request.MemberId;
            current.CategoryId = request.CategoryId;
            current.Kind = request.Kind;
            current.Amount = request.Amount;
            current.MerchantName = request.MerchantName.Trim();
            current.PaymentMethod = request.PaymentMethod.Trim();
            current.Note = request.Note.Trim();
            current.OccurredAt = request.OccurredAt ?? current.OccurredAt;

            await _transactionService.UpdateAsync(current);
        }

        /// <summary>
        /// 删除正式账单。
        /// </summary>
        public Task DeleteTransactionAsync(string id, CancellationToken cancellationToken = default)
            => _transactionService.DeleteAsync(id);

        /// <summary>
        /// 预览截图 OCR 识别结果。
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
                var result = await _captureRecognitionService.AnalyzeAsync(tempFilePath, existingText, existingAmount, existingMerchant, cancellationToken);
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
        /// 识别截图并直接保存正式账单。
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
                return await _captureRecognitionService.AnalyzeAsync(tempFilePath, existingText, existingAmount, existingMerchant, cancellationToken);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

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

        private static FamilyMemberDto MapMember(FamilyMember member) => new()
        {
            Id = member.Id,
            Name = member.Name,
            Role = member.Role,
            AccentColor = member.AccentColor
        };

        private static LedgerCategoryDto MapCategory(LedgerCategory category) => new()
        {
            Id = category.Id,
            Name = category.Name,
            Kind = category.Kind,
            Color = category.Color
        };

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
    }
}
