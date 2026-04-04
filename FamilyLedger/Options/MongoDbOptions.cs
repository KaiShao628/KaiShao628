namespace FamilyLedger.Options;

public sealed class MongoDbOptions
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "FamilyLedger";
    public string TransactionsCollectionName { get; set; } = "transactions";
    public string MembersCollectionName { get; set; } = "members";
    public string CategoriesCollectionName { get; set; } = "categories";
    public string CaptureDraftsCollectionName { get; set; } = "captureDrafts";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ConnectionString) &&
        !string.IsNullOrWhiteSpace(DatabaseName);
}
