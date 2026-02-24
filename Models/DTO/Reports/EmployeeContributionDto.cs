// Models/DTO/Reports/EmployeeContributionDto.cs
namespace backend_trial.Models.DTO.Reports
{
    public class EmployeeContributionDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int IdeasSubmitted { get; set; }
        public int IdeasApproved { get; set; }
        public int CommentsPosted { get; set; }
        public int VotesGiven { get; set; }
        public decimal ApprovalRate { get; set; }
    }
}
