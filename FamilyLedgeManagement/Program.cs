using FamilyLedgeManagement.Components;
using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Options;
using FamilyLedgeManagement.Repositories;
using FamilyLedgeManagement.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System.Globalization;
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
builder.Services.AddScoped<StructuredCaptureRecognitionService>();
builder.Services.AddScoped<LedgerService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.MapStaticAssets();
var uploadRoot = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadRoot),
    RequestPath = "/uploads"
});
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapPost("/api/family-ledger/quick-entry", async (QuickEntryRequestDto request, LedgerService ledgerService, CancellationToken cancellationToken) =>
{
    var transaction = await ledgerService.AddTransactionAsync(request, cancellationToken);
    return Results.Created($"/api/family-ledger/transactions/{transaction.Id}", transaction);
}).DisableAntiforgery();

app.MapPost("/api/family-ledger/capture-drafts/upload", async (HttpRequest request, LedgerService ledgerService, CancellationToken cancellationToken) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new { message = "请使用 multipart/form-data 上传截图。" });
    }

    var form = await request.ReadFormAsync(cancellationToken);
    var file = form.Files["file"] ?? form.Files.FirstOrDefault();

    if (file is null || file.Length == 0)
    {
        return Results.BadRequest(new { message = "请至少上传一张截图图片。" });
    }

    decimal? suggestedAmount = null;
    if (decimal.TryParse(form["suggestedAmount"], NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedAmount))
    {
        suggestedAmount = parsedAmount;
    }

    DateTimeOffset? capturedAt = null;
    if (DateTimeOffset.TryParse(form["capturedAt"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedCapturedAt))
    {
        capturedAt = parsedCapturedAt;
    }

    var draftRequest = new CaptureDraftRequestDto
    {
        MemberId = form["memberId"].ToString(),
        SuggestedCategoryId = form["suggestedCategoryId"].ToString(),
        SuggestedAmount = suggestedAmount,
        MerchantName = form["merchantName"].ToString(),
        Source = string.IsNullOrWhiteSpace(form["source"]) ? "快捷指令上传" : form["source"].ToString(),
        RecognizedText = form["recognizedText"].ToString(),
        CapturedAt = capturedAt
    };

    await using var stream = file.OpenReadStream();
    var transaction = await ledgerService.AddTransactionFromCaptureAsync(draftRequest, stream, file.FileName, cancellationToken);
    return Results.Created($"/api/family-ledger/transactions/{transaction.Id}", transaction);
}).DisableAntiforgery();

using (var scope = app.Services.CreateScope())
{
    var ledgerService = scope.ServiceProvider.GetRequiredService<LedgerService>();
    await ledgerService.EnsureSeedDataAsync();
}

app.Run();

