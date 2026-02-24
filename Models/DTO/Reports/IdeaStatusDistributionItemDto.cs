// Models/DTO/Reports/IdeaStatusDistributionItemDto.cs
namespace backend_trial.Models.DTO.Reports
{
    public class IdeaStatusDistributionItemDto
    {
        public string Status { get; set; } = default!;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}