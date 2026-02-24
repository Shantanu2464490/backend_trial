// Models/DTO/Reports/IdeasByDateDto.cs
namespace backend_trial.Models.DTO.Reports
{
    public class IdeasByDateDto
    {
        public string Date { get; set; } = default!; // yyyy-MM-dd
        public int IdeasSubmitted { get; set; }
        public int IdeasApproved { get; set; }
        public int IdeasRejected { get; set; }
        public int IdeasUnderReview { get; set; }
    }
}