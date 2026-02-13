namespace backend_trial.Models.DTO
{
    public class UpdateNotificationStatusRequestDto
    {
        public Guid NotificationId { get; set; }
        public string Status { get; set; } = null!;
    }
}