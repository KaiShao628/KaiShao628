namespace FamilyLedgeManagement.Dtos.BaseDtos
{
    public class FilterDto
    {
        public string? SearchText { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 100;

        public long TotalPages { get; set; } = 0;

        public string DicId { get; set; }
        public string ProjectId { get; set; }
        public string TeamId { get; set; }
        public string TaskId { get; set; }

        public string LoginUserId { get; set; }

        public bool IsSuperAdmin { get; set; }

        public DateTimeOffset TransactionStratDate { get; set; }
        public DateTimeOffset TransactionEndDate { get; set; }
    }
}
