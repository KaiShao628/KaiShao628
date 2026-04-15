using Microsoft.AspNetCore.Components;

namespace FamilyLedgeManagement.Components.Pages.NormalPages
{
    public partial class CloudWarpper
    {
        /// <summary>
        /// 获得/设置 子组件
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// 不主动刷新ui
        /// </summary>
        [Parameter]
        public bool NotRender { get; set; }

        /// <summary>
        /// 不需要包裹div
        /// </summary>
        [Parameter]
        public bool WithoutDicWarpper { get; set; }

        /// <summary>
        /// 不主动刷新ui
        /// </summary>
        [Parameter]
        public string Name { get; set; }

        /// <summary>
        /// 刷新ui时弹窗
        /// </summary>
        [Parameter]
        public bool ToastWhenRefresh { get; set; }

        bool _shouldRender = true;
        protected async override Task OnParametersSetAsync()
        {
            _shouldRender = !NotRender;
        }

        public void UpdateUI()
        {
            try
            {
                bool temp = _shouldRender;
                _shouldRender = true;
                StateHasChanged();
                _shouldRender = temp;
            }
            catch (Exception)
            {

            }

        }

        protected override bool ShouldRender()
        {
            return _shouldRender;
        }
    }
}