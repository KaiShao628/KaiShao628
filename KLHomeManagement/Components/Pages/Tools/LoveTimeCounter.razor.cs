using Microsoft.AspNetCore.Components;

namespace KLHomeManagement.Components.Pages.Tools
{
    public partial class LoveTimeCounter
    {
        /// <summary>
        /// 计时器标题（外部传入）
        /// </summary>
        [Parameter]
        public string Title { get; set; } = "计时器";

        /// <summary>
        /// 开始时间（外部传入）
        /// </summary>
        [Parameter]
        public DateTime StartTime { get; set; } = DateTime.Now;

        // 存储计算出的时间差
        private TimeSpan _timeSpan;

        // 定时器，用于每秒更新
        private System.Timers.Timer? _timer;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // 初始化计算一次时间差
            CalculateTimeDifference();

            // 设置定时器，每秒触发一次
            _timer = new System.Timers.Timer(1000); // 1000毫秒 = 1秒
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        /// <summary>
        /// 定时器触发事件，计算并更新时间差
        /// </summary>
        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            CalculateTimeDifference();

            // 通知Blazor更新UI（必须在UI线程执行）
            InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// 核心计算逻辑：当前时间 - 开始时间
        /// </summary>
        private void CalculateTimeDifference()
        {
            // 获取当前UTC时间（避免时区问题）
            var currentTime = DateTime.UtcNow;

            // 计算时间差
            _timeSpan = currentTime - StartTime;

            // 确保时间差为正数（如果开始时间大于当前时间，显示0）
            if (_timeSpan < TimeSpan.Zero)
            {
                _timeSpan = TimeSpan.Zero;
            }
        }

        /// <summary>
        /// 组件销毁时释放定时器资源
        /// </summary>
        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Elapsed -= Timer_Elapsed;
                _timer.Stop();
                _timer.Dispose();
            }
        }
    }
}