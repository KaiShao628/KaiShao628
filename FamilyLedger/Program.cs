using FamilyLedger.Components;
using FamilyLedger.Models;
using FamilyLedger.Options;
using FamilyLedger.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBootstrapBlazor();
builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection(MongoDbOptions.SectionName));
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<ILedgerRepository>(sp =>
{
    var options = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;
    return options.IsConfigured
        ? new MongoLedgerRepository(options)
        : new InMemoryLedgerRepository(sp.GetRequiredService<TimeProvider>());
});
builder.Services.AddScoped<LedgerService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/api/family-ledger/quick-entry", async (QuickEntryRequest request, LedgerService ledgerService, CancellationToken cancellationToken) =>
{
    var transaction = await ledgerService.AddTransactionAsync(request, cancellationToken);
    return Results.Created($"/api/family-ledger/transactions/{transaction.Id}", transaction);
});

app.MapPost("/api/family-ledger/capture-drafts", async (CaptureDraftRequest request, LedgerService ledgerService, CancellationToken cancellationToken) =>
{
    var draft = await ledgerService.AddCaptureDraftAsync(request, cancellationToken);
    return Results.Created($"/api/family-ledger/capture-drafts/{draft.Id}", draft);
});

using (var scope = app.Services.CreateScope())
{
    var ledgerService = scope.ServiceProvider.GetRequiredService<LedgerService>();
    await ledgerService.EnsureSeedDataAsync();
}

app.Run();
