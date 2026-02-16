namespace backend_trial.Models.DTO
{
    public class NotificationResponseDto
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public Guid? IdeaId { get; set; }
        public string? IdeaTitle { get; set; }
        public Guid? ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
    }
}