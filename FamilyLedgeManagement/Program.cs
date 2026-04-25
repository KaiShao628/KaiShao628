using BootstrapBlazor.Components;
using FamilyLedgeManagement.Components;
using FamilyLedgeManagement.Database;
using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Services;
using FamilyLedgeManagement.Utilities;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var supportedCultures = new List<string> { "zh-CN", "en-US" };
var localeDirectory = Path.Combine(builder.Environment.ContentRootPath, "Locales");
var localeFiles = supportedCultures
    .Select(culture => Path.Combine(localeDirectory, $"{culture}.json"))
    .ToArray();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

KLFamilyLedgeAppSettingsHelper.Initialization();
FamilyLedgeMongoDBClient.Instance.StartServer();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddLocalization();
builder.Services.AddBootstrapBlazor(options =>
{
    options.ToastDelay = 4000;
    options.SupportedCultures = supportedCultures;
    options.FallbackCulture = "zh-CN";
}, localizationOptions =>
{
    localizationOptions.AdditionalJsonFiles = localeFiles;
    localizationOptions.IgnoreLocalizerMissing = false;
    localizationOptions.UseKeyWhenValueIsNull = true;
});
builder.Services.AddBootstrapBlazorTableExportService();
builder.Services.AddBootstrapBlazorHtml2PdfService();
//builder.Services.Configure<RequestLocalizationOptions>(options =>
//{
//    options.SetDefaultCulture("zh-CN")
//        .AddSupportedCultures(supportedCultures)
//        .AddSupportedUICultures(supportedCultures);

//    options.RequestCultureProviders = new List<IRequestCultureProvider>
//    {
//        new QueryStringRequestCultureProvider(),
//        new CookieRequestCultureProvider(),
//        new AcceptLanguageHeaderRequestCultureProvider()
//    };
//});
builder.Services.Configure<HubOptions>(option => option.MaximumReceiveMessageSize = null);
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddProjectServices();
builder.Services.AddScoped<StructuredCaptureRecognitionService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// 启用本地化
app.UseRequestLocalization(app.Services.GetService<IOptions<RequestLocalizationOptions>>()!.Value);

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
app.MapGet("/culture/set", (string culture, string? redirectUri, HttpContext context) =>
{
    if (!supportedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase))
    {
        return Results.BadRequest(new { message = $"不支持的语言：{culture}" });
    }

    context.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
        new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        });

    var targetUri = string.IsNullOrWhiteSpace(redirectUri) ? "/" : redirectUri;
    return Results.LocalRedirect(targetUri);
}).DisableAntiforgery();

app.MapPost("/api/family-ledger/quick-entry", async (QuickEntryRequestDto request, LedgerService ledgerService, CancellationToken cancellationToken) =>
{
    var transaction = await ledgerService.AddTransactionAsync(request, cancellationToken);
    return Results.Created($"/api/family-ledger/transactions/{transaction.Id}", transaction);
}).DisableAntiforgery();

app.MapPost("/api/family-ledger/capture/upload", async (HttpRequest request, LedgerService ledgerService, CancellationToken cancellationToken) =>
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

    var captureRequest = new CaptureEntryRequestDto
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
    var transaction = await ledgerService.AddTransactionFromCaptureAsync(captureRequest, stream, file.FileName, cancellationToken);
    return Results.Created($"/api/family-ledger/transactions/{transaction.Id}", transaction);
}).DisableAntiforgery();

app.Run();
