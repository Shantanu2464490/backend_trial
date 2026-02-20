using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Xml.Linq;

namespace backend_trial.Models.Domain
{
    public class Idea
    {
        public Guid IdeaId { get; set; } 
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Guid CategoryId { get; set; }
        public Guid SubmittedByUserId { get; set; }
        public DateTime SubmittedDate { get; set; }
        public IdeaStatus Status { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public string? ReviewedByUserName { get; set; }
        public DateTime? ReviewedDate { get; set; }
        
        public Category Category { get; set; } = null!;
        public User SubmittedByUser { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
