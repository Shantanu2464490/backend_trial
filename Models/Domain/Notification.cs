namespace backend_trial.Models.Domain
{
    public class Notification
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; } = null!;
        public NotificationStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? IdeaId { get; set; }
        public Guid? ReviewerId { get; set; }

        public User User { get; set; } = null!;
        public Idea? Idea { get; set; }
        public User? Reviewer { get; set; }
    }
}
