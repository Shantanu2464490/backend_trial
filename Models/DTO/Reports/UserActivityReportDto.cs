// Models/DTO/Reports/UserActivityReportDto.cs
namespace backend_trial.Models.DTO.Reports
{
    public class UserActivityReportDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int UsersWithIdeas { get; set; }
        public int UsersWithReviews { get; set; }
        public int UsersWithComments { get; set; }
        public int UsersWithVotes { get; set; }
        public decimal AverageIdeasPerUser { get; set; }
        public decimal AverageCommentsPerUser { get; set; }
        public decimal EngagementRate { get; set; }
    }
}