using FamilyLedgeManagement.Components;
using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Options;
using FamilyLedgeManagement.Repositories;
using FamilyLedgeManagement.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddBootstrapBlazor();
builder.Services.AddBootstrapBlazorTableExportService();
builder.Services.AddBootstrapBlazorHtml2PdfService();
builder.Services.Configure<HubOptions>(option => option.MaximumReceiveMessageSize = null);
builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection(MongoDbOptions.SectionName));
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<ILedgerRepository>(sp =>
{
    var options = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;
    return options.IsConfigured
        ? new MongoLedgerRepository(options)
        : new InMemoryLedgerRepository();
});
builder.Services.AddScoped<LedgerService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapPost("/api/family-ledger/quick-entry", async (QuickEntryRequestDto request, LedgerService ledgerService, CancellationToken cancellationToken) =>
{
    var transaction = await ledgerService.AddTransactionAsync(request, cancellationToken);
    return Results.Created($"/api/family-ledger/transactions/{transaction.Id}", transaction);
});

app.MapPost("/api/family-ledger/capture-drafts", async (CaptureDraftRequestDto request, LedgerService ledgerService, CancellationToken cancellationToken) =>
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

