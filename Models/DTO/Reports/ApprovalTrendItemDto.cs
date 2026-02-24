// Models/DTO/Reports/ApprovalTrendItemDto.cs
namespace backend_trial.Models.DTO.Reports
{
    public class ApprovalTrendItemDto
    {
        public string Month { get; set; } = default!; // yyyy-MM
        public int IdeasSubmitted { get; set; }
        public int IdeasApproved { get; set; }
        public decimal ApprovalRate { get; set; }
    }
}