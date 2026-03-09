using System.ComponentModel.DataAnnotations;

namespace backend_trial.Models.DTO.Idea
{
    public class IdeaRequestDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Title must be above 5 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be above 10 characters")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Category is required")]
        public Guid CategoryId { get; set; }
    }
}
