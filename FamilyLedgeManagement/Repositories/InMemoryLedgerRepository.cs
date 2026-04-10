using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Repositories;

public sealed class InMemoryLedgerRepository : ILedgerRepository
{
    private readonly Lock _gate = new();
    private readonly List<FamilyMember> _members = [];
    private readonly List<LedgerCategory> _categories = [];
    private readonly List<LedgerTransaction> _transactions = [];
    private readonly List<CaptureDraft> _captureDrafts = [];

    public Task<IReadOnlyList<FamilyMember>> GetMembersAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<FamilyMember>>(_members.Select(Clone).ToList());
        }
    }

    public Task SaveMembersAsync(IEnumerable<FamilyMember> members, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _members.Clear();
            _members.AddRange(members.Select(Clone));
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<LedgerCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<LedgerCategory>>(_categories.Select(Clone).ToList());
        }
    }

    public Task SaveCategoriesAsync(IEnumerable<LedgerCategory> categories, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _categories.Clear();
            _categories.AddRange(categories.Select(Clone));
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<LedgerTransaction>> GetTransactionsAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<LedgerTransaction>>(_transactions.Select(Clone).ToList());
        }
    }

    public Task<LedgerTransaction?> GetTransactionAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_transactions.Where(x => x.Id == id).Select(Clone).FirstOrDefault());
        }
    }

    public Task AddTransactionAsync(LedgerTransaction transaction, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _transactions.Add(Clone(transaction));
        }
        return Task.CompletedTask;
    }

    public Task UpdateTransactionAsync(LedgerTransaction transaction, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var index = _transactions.FindIndex(x => x.Id == transaction.Id);
            if (index >= 0)
            {
                _transactions[index] = Clone(transaction);
            }
        }

        return Task.CompletedTask;
    }

    public Task DeleteTransactionAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _transactions.RemoveAll(x => x.Id == id);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CaptureDraft>> GetCaptureDraftsAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<CaptureDraft>>(_captureDrafts.Select(Clone).ToList());
        }
    }

    public Task<CaptureDraft?> GetCaptureDraftAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_captureDrafts.Where(x => x.Id == id).Select(Clone).FirstOrDefault());
        }
    }

    public Task AddCaptureDraftAsync(CaptureDraft draft, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _captureDrafts.Add(Clone(draft));
        }
        return Task.CompletedTask;
    }

    public Task UpdateCaptureDraftAsync(CaptureDraft draft, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var index = _captureDrafts.FindIndex(x => x.Id == draft.Id);
            if (index >= 0)
            {
                _captureDrafts[index] = Clone(draft);
            }
        }
        return Task.CompletedTask;
    }

    private static FamilyMember Clone(FamilyMember item) => new() { Id = item.Id, Name = item.Name, Role = item.Role, AccentColor = item.AccentColor };
    private static LedgerCategory Clone(LedgerCategory item) => new() { Id = item.Id, Name = item.Name, Kind = item.Kind, Color = item.Color };
    private static LedgerTransaction Clone(LedgerTransaction item) => new()
    {
        Id = item.Id,
        MemberId = item.MemberId,
        CategoryId = item.CategoryId,
        Kind = item.Kind,
        Amount = item.Amount,
        MerchantName = item.MerchantName,
        PaymentMethod = item.PaymentMethod,
        Note = item.Note,
        OccurredAt = item.OccurredAt,
        CreatedAt = item.CreatedAt
    };
    private static CaptureDraft Clone(CaptureDraft item) => new()
    {
        Id = item.Id,
        MemberId = item.MemberId,
        SuggestedCategoryId = item.SuggestedCategoryId,
        SuggestedAmount = item.SuggestedAmount,
        MerchantName = item.MerchantName,
        ProductName = item.ProductName,
        PaymentMethod = item.PaymentMethod,
        Source = item.Source,
        RecognizedText = item.RecognizedText,
        ImageUrl = item.ImageUrl,
        OriginalFileName = item.OriginalFileName,
        RecognizedOccurredAt = item.RecognizedOccurredAt,
        CapturedAt = item.CapturedAt,
        Status = item.Status
    };
}

