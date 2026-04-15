using BootstrapBlazor.Components;

namespace FamilyLedgeManagement.Utilities
{
    public class FamilyLedgeMessageHelper
    {
        public static string SaveTitle = "保存数据";
        public static string SaveSuccessContent = "保存数据成功, 4 秒后自动关闭";
        public static string SaveErrorContent = "保存数据失败, 4 秒后自动关闭";
        public static string DeleteTitle = "删除数据";
        public static string DeleteSuccessContent = "删除数据成功, 4 秒后自动关闭";
        public static string DeleteErrorContent = "删除数据失败, 4 秒后自动关闭";
        public static string ContentTemplate = ", 4 秒后自动关闭";

        private MessageService MessageService { get; set; }
        private ToastService ToastService { get; set; }

        public FamilyLedgeMessageHelper(MessageService messageService, ToastService toastService)
        {
            MessageService = messageService;
            ToastService = toastService;
        }

        #region MessageService
        public async Task MessageSuccessAsync(string msg)
        {
            await MessageService.Show(new MessageOption { Content = msg, Color = Color.Success });
        }

        public async Task MessageWarningAsync(string msg)
        {
            await MessageService.Show(new MessageOption { Content = msg, Color = Color.Warning });
        }

        public async Task MessageDangerAsync(string msg)
        {
            await MessageService.Show(new MessageOption { Content = msg, Color = Color.Danger });
        }

        public async Task MessageInfoAsync(string msg)
        {
            await MessageService.Show(new MessageOption { Content = msg, Color = Color.Info });
        }
        #endregion

        #region ToastService
        public async Task TosatSuccessAsync(string title, string msg)
        {
            await ToastService.Show(new ToastOption() { Category = ToastCategory.Success, Title = title, Content = msg });
        }

        public async Task TosatWarningAsync(string title, string msg)
        {
            await ToastService.Show(new ToastOption() { Category = ToastCategory.Warning, Title = title, Content = msg });
        }

        public async Task TosatErrorAsync(string title, string msg)
        {
            await ToastService.Show(new ToastOption() { Category = ToastCategory.Error, Title = title, Content = msg });
        }

        public async Task TosatInfoAsync(string title, string msg)
        {
            await ToastService.Show(new ToastOption() { Category = ToastCategory.Information, Title = title, Content = msg });
        }
        #endregion
    }
}
