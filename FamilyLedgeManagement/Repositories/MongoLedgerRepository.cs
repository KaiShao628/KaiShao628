using FamilyLedgeManagement.Models;
using FamilyLedgeManagement.Options;
using MongoDB.Driver;

namespace FamilyLedgeManagement.Repositories;

public sealed class MongoLedgerRepository : ILedgerRepository
{
    private readonly IMongoCollection<FamilyMember> _members;
    private readonly IMongoCollection<LedgerCategory> _categories;
    private readonly IMongoCollection<LedgerTransaction> _transactions;
    private readonly IMongoCollection<CaptureDraft> _captureDrafts;

    public MongoLedgerRepository(MongoDbOptions options)
    {
        var client = new MongoClient(options.ConnectionString);
        var database = client.GetDatabase(options.DatabaseName);

        _members = database.GetCollection<FamilyMember>(options.MembersCollectionName);
        _categories = database.GetCollection<LedgerCategory>(options.CategoriesCollectionName);
        _transactions = database.GetCollection<LedgerTransaction>(options.TransactionsCollectionName);
        _captureDrafts = database.GetCollection<CaptureDraft>(options.CaptureDraftsCollectionName);
    }

    public async Task<IReadOnlyList<FamilyMember>> GetMembersAsync(CancellationToken cancellationToken = default) =>
        await _members.Find(FilterDefinition<FamilyMember>.Empty).ToListAsync(cancellationToken);

    public async Task SaveMembersAsync(IEnumerable<FamilyMember> members, CancellationToken cancellationToken = default)
    {
        await _members.DeleteManyAsync(FilterDefinition<FamilyMember>.Empty, cancellationToken);
        if (members.Any())
        {
            await _members.InsertManyAsync(members, cancellationToken: cancellationToken);
        }
    }

    public async Task<IReadOnlyList<LedgerCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
        await _categories.Find(FilterDefinition<LedgerCategory>.Empty).ToListAsync(cancellationToken);

    public async Task SaveCategoriesAsync(IEnumerable<LedgerCategory> categories, CancellationToken cancellationToken = default)
    {
        await _categories.DeleteManyAsync(FilterDefinition<LedgerCategory>.Empty, cancellationToken);
        if (categories.Any())
        {
            await _categories.InsertManyAsync(categories, cancellationToken: cancellationToken);
        }
    }

    public async Task<IReadOnlyList<LedgerTransaction>> GetTransactionsAsync(CancellationToken cancellationToken = default) =>
        await _transactions.Find(FilterDefinition<LedgerTransaction>.Empty).SortByDescending(x => x.OccurredAt).ToListAsync(cancellationToken);

    public async Task<LedgerTransaction?> GetTransactionAsync(string id, CancellationToken cancellationToken = default) =>
        await _transactions.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

    public Task AddTransactionAsync(LedgerTransaction transaction, CancellationToken cancellationToken = default) =>
        _transactions.InsertOneAsync(transaction, cancellationToken: cancellationToken);

    public Task UpdateTransactionAsync(LedgerTransaction transaction, CancellationToken cancellationToken = default) =>
        _transactions.ReplaceOneAsync(x => x.Id == transaction.Id, transaction, cancellationToken: cancellationToken);

    public Task DeleteTransactionAsync(string id, CancellationToken cancellationToken = default) =>
        _transactions.DeleteOneAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CaptureDraft>> GetCaptureDraftsAsync(CancellationToken cancellationToken = default) =>
        await _captureDrafts.Find(FilterDefinition<CaptureDraft>.Empty).SortByDescending(x => x.CapturedAt).ToListAsync(cancellationToken);

    public async Task<CaptureDraft?> GetCaptureDraftAsync(string id, CancellationToken cancellationToken = default) =>
        await _captureDrafts.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

    public Task AddCaptureDraftAsync(CaptureDraft draft, CancellationToken cancellationToken = default) =>
        _captureDrafts.InsertOneAsync(draft, cancellationToken: cancellationToken);

    public Task UpdateCaptureDraftAsync(CaptureDraft draft, CancellationToken cancellationToken = default) =>
        _captureDrafts.ReplaceOneAsync(x => x.Id == draft.Id, draft, cancellationToken: cancellationToken);
}

