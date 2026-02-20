namespace backend_trial.Models.Domain
{
    public class Review
    {
        public Guid ReviewId { get; set; } 

        public Guid IdeaId { get; set; }
        public Guid ReviewerId { get; set; }

        public string Feedback { get; set; } = null!;
        public ReviewDecision Decision { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime ReviewDate { get; set; }

        public Idea Idea { get; set; } = null!;
        public User Reviewer { get; set; } = null!;
    }
}
