namespace backend_trial.Models.DTO
{
    public class ReviewSubmitDto
    {
        public Guid IdeaId { get; set; }
        public string Feedback { get; set; } = null!;
        public string Decision { get; set; } = null!; // "Approve" or "Reject"
        public string? RejectionReason { get; set; } // NEW - Mandatory for rejections
    }
}