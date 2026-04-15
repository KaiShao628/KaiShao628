namespace FamilyLedgeManagement.Enums;

/// <summary>
/// 截图识别记录处理状态。
/// </summary>
public enum CaptureDraftStatus
{
    /// <summary>
    /// 待处理。
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已确认入账。
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// 已忽略。
    /// </summary>
    Ignored = 2
}
