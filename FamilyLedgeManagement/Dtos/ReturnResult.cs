using FamilyLedgeManagement.Dtos.BaseDtos;
using FamilyLedgeManagement.Enums;

namespace FamilyLedgeManagement.Dtos
{
    /// <summary>
    /// 通用返回结果模型。
    /// </summary>
    public class ReturnResult<T>
    {
        /// <summary>
        /// 数据库操作状态。
        /// </summary>
        public DbOpStatus Status { get; set; }

        /// <summary>
        /// 提示状态码。
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 提示信息或错误信息。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 返回数据。
        /// </summary>
        public T? Data { get; set; }
    }

    public class TableResultDto<T> where T : EntityBaseDto
    {
        public List<T> Result { get; set; } = new List<T>();

        public int TotalPages { get; set; } = 0;
    }


}
