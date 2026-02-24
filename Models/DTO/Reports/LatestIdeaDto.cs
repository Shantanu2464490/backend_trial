// Models/DTO/Reports/LatestIdeaDto.cs
namespace backend_trial.Models.DTO.Reports
{
    public class LatestIdeaDto
    {
        public Guid IdeaId { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public string SubmittedBy { get; set; } = default!;
        public string CategoryName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public DateTime SubmittedDate { get; set; }
    }
}
