using BootstrapBlazor.Components;
using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Models;
using FamilyLedgeManagement.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FamilyLedgeManagement.Components.Pages;

public partial class CaptureInbox
{
    [Inject]
    private LedgerService LedgerService { get; set; } = default!;

    protected QuickEntryContextDto? Context { get; set; }
    protected CaptureDraftRequestDto UploadRequest { get; set; } = new();
    protected CaptureRecognitionPreviewDto? RecognitionPreview { get; set; }
    protected TransactionListItemDto? LatestSavedTransaction { get; set; }
    protected UploadFile? SelectedUploadFile { get; set; }
    protected string UploadKey { get; set; } = Guid.NewGuid().ToString("N");
    protected string? Message { get; set; }
    protected string? ErrorMessage { get; set; }

    protected IEnumerable<LedgerCategoryDto> ExpenseCategories =>
        Context?.Categories.Where(x => x.Kind == TransactionKind.Expense) ?? Enumerable.Empty<LedgerCategoryDto>();

    protected override async Task OnInitializedAsync()
    {
        Context = await LedgerService.GetQuickEntryContextAsync();
        ResetUploadRequest();
    }

    protected Task OnUploadChanged(UploadFile file)
        => OnUploadChangedAsync(file);

    protected async Task SaveUploadAsync(EditContext _)
    {
        Message = null;
        ErrorMessage = null;

        if (SelectedUploadFile?.File is null)
        {
            ErrorMessage = "请先选择一张截图图片。";
            return;
        }

        try
        {
            await using var stream = SelectedUploadFile.File.OpenReadStream(10 * 1024 * 1024);
            LatestSavedTransaction = await LedgerService.AddTransactionFromCaptureAsync(
                UploadRequest,
                stream,
                SelectedUploadFile.OriginFileName ?? SelectedUploadFile.FileName ?? "capture.png");

            Message = $"已自动保存支出：{LatestSavedTransaction.MerchantName} {LatestSavedTransaction.Amount:F2}";
            ResetUploadRequest();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private async Task OnUploadChangedAsync(UploadFile file)
    {
        SelectedUploadFile = file;
        ErrorMessage = null;
        Message = null;

        if (SelectedUploadFile?.File is null)
        {
            return;
        }

        try
        {
            await using var stream = SelectedUploadFile.File.OpenReadStream(10 * 1024 * 1024);
            var preview = await LedgerService.PreviewCaptureRecognitionAsync(
                stream,
                SelectedUploadFile.OriginFileName ?? SelectedUploadFile.FileName ?? "capture.png",
                UploadRequest.RecognizedText,
                UploadRequest.SuggestedAmount,
                UploadRequest.MerchantName);

            RecognitionPreview = preview;
            UploadRequest.SuggestedAmount = preview.SuggestedAmount;
            UploadRequest.MerchantName = preview.MerchantName;
            UploadRequest.RecognizedText = preview.RecognizedText;
            UploadRequest.CapturedAt = preview.OccurredAt ?? UploadRequest.CapturedAt;

            Message = preview.SuggestedAmount is null
                ? "图片已完成预识别，但暂时没有识别出金额，请检查识别文本。"
                : $"OCR 已识别金额 {preview.SuggestedAmount:F2}，请确认后再上传到收件箱。";

            StateHasChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"截图预识别失败：{ex.Message}";
        }
    }

    private void ResetUploadRequest()
    {
        if (Context is null)
        {
            return;
        }

        UploadRequest = new CaptureDraftRequestDto
        {
            MemberId = Context.Members.First().Id,
            SuggestedCategoryId = ExpenseCategories.First().Id,
            Source = "页面上传",
            CapturedAt = DateTimeOffset.Now
        };
        RecognitionPreview = null;
        LatestSavedTransaction = null;
        SelectedUploadFile = null;
        UploadKey = Guid.NewGuid().ToString("N");
    }
}
