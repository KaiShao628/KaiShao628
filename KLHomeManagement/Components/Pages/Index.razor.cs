
using System.ComponentModel;

namespace KLHomeManagement.Components.Pages
{
    public partial class Index
    {
        private List<MemoryDay> MemoryDays { get; set; } = new List<MemoryDay>();
        // 存储计算出的时间差
        private TimeSpan _timeSpan;

        // 定时器，用于每秒更新
        private System.Timers.Timer? _timer;

        protected override async Task OnInitializedAsync()
        {
            MemoryDays = new List<MemoryDay>
            {
                new MemoryDay("距恋爱纪念日",new DateTime(2017,2,1)),
                new MemoryDay("距领证纪念日",new DateTime(2020,10,20)),
                new MemoryDay("距结婚纪念日",new DateTime(2025,10,3)),
            };
            // 初始化计算一次时间差
            CalculateTimeDifference(MemoryDays);

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
            CalculateTimeDifference(MemoryDays);

            // 通知Blazor更新UI（必须在UI线程执行）
            InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// 核心计算逻辑：当前时间 - 开始时间
        /// </summary>
        private void CalculateTimeDifference(List<MemoryDay> memoryDays)
        {
            // 获取当前UTC时间（避免时区问题），也可以用DateTime.Now
            var currentTime = DateTime.Now;

            foreach (var memoryDay in memoryDays)
            {
                // 计算时间差
                var timeSpan = currentTime - memoryDay.DayTime;

                // 确保时间差为正数（如果开始时间大于当前时间，显示0）
                if (timeSpan < TimeSpan.Zero)
                {
                    timeSpan = TimeSpan.Zero;
                }

                memoryDay.TimeSpan = timeSpan;
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

        public class MemoryDay
        {
            public string DayName { get; set; }
            public DateTime DayTime { get; set; }

            public TimeSpan TimeSpan { get; set; }

            public MemoryDay(string dayName, DateTime date)
            {
                DayName = dayName;
                DayTime = date;
            }
        }
    }
}