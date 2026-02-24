// Models/DTO/Reports/CategoryReportDto.cs
namespace backend_trial.Models.DTO.Reports
{
    public class CategoryReportDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = default!;
        public int IdeasSubmitted { get; set; }
        public int ApprovedIdeas { get; set; }
        public int RejectedIdeas { get; set; }
        public int UnderReviewIdeas { get; set; }
        public decimal ApprovalRate { get; set; }
    }
}