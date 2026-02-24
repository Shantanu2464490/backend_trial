
namespace backend_trial.Models.DTO.Reports
{
    public class SystemOverviewDto
    {
        public int TotalIdeas { get; set; }
        public int TotalApprovedIdeas { get; set; }
        public int TotalRejectedIdeas { get; set; }
        public int TotalUnderReviewIdeas { get; set; }

        public int TotalUsers { get; set; }
        public int TotalManagers { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalAdmins { get; set; }

        public int TotalCategories { get; set; }
        public int ActiveCategories { get; set; }

        public decimal ApprovalRate { get; set; }
        public IEnumerable<IdeaStatusDistributionItemDto> IdeaStatusDistribution { get; set; } = Enumerable.Empty<IdeaStatusDistributionItemDto>();
        public IEnumerable<CategoryReportDto> CategoryReports { get; set; } = Enumerable.Empty<CategoryReportDto>();
    }
}