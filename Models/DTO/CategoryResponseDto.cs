// backend_trial/Models/DTO/CategoryResponseDto.cs
namespace backend_trial.Models.DTO
{
    public class CategoryResponseDto
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}