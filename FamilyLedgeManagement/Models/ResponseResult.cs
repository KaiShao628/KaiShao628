using FamilyLedgeManagement.Dtos;
using FamilyLedgeManagement.Enums;
using System.Net.NetworkInformation;

namespace FamilyLedgeManagement.Models
{
    public class ResponseResult<T> : ReturnResult<T>
    {
        public ResponseResult()
        {
            this.StatusCode = (int)DbOpStatus.Success;
            this.Message = DbOpStatus.Success.ToString();
            this.Data = default;
            Status = DbOpStatus.Success;
        }

        public ResponseResult(DbOpStatus status, string msg, T data)
        {
            this.StatusCode = (int)status;
            this.Message = msg;
            this.Data = data;
            Status = status;
        }
        public ResponseResult(DbOpStatus status, string msg)
        {
            this.StatusCode = (int)status;
            this.Message = msg;
            this.Data = default;
            Status = status;
        }
        public ResponseResult(T data)
        {
            this.StatusCode = (int)DbOpStatus.Success;
            this.Message = DbOpStatus.Success.ToString();
            this.Data = data;
            Status = DbOpStatus.Success;
        }
        public ResponseResult(DbOpStatus status, T data)
        {
            this.StatusCode = (int)status;
            this.Message = DbOpStatus.Success.ToString();
            this.Data = data;
            Status = status;
        }
        public ResponseResult(DbOpStatus status)
        {
            this.StatusCode = (int)status;
            this.Message = DbOpStatus.Success.ToString();
            this.Data = default;
            Status = status;
        }
    }
}
