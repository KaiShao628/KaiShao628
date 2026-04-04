using FamilyLedger.Models;

namespace FamilyLedger.Services;

public interface ILedgerRepository
{
    Task<IReadOnlyList<FamilyMember>> GetMembersAsync(CancellationToken cancellationToken = default);
    Task SaveMembersAsync(IEnumerable<FamilyMember> members, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LedgerCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task SaveCategoriesAsync(IEnumerable<LedgerCategory> categories, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LedgerTransaction>> GetTransactionsAsync(CancellationToken cancellationToken = default);
    Task AddTransactionAsync(LedgerTransaction transaction, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CaptureDraft>> GetCaptureDraftsAsync(CancellationToken cancellationToken = default);
    Task<CaptureDraft?> GetCaptureDraftAsync(string id, CancellationToken cancellationToken = default);
    Task AddCaptureDraftAsync(CaptureDraft draft, CancellationToken cancellationToken = default);
    Task UpdateCaptureDraftAsync(CaptureDraft draft, CancellationToken cancellationToken = default);
}
