using backend_trial.Models.Domain;
using backend_trial.Models.DTO.Comment;
using backend_trial.Models.DTO.Idea;
using backend_trial.Repositories.Interfaces;
using backend_trial.Services.Interfaces;

namespace backend_trial.Services
{
    public class IdeaService : IIdeaService
    {
        private readonly IIdeaRepository ideaRepository;
        private readonly INotificationService notificationService;

        public IdeaService(IIdeaRepository ideaRepository, INotificationService notificationService)
        {
            this.ideaRepository = ideaRepository;
            this.notificationService = notificationService;
        }

        public async Task<IEnumerable<IdeaResponseDto>> GetAllIdeasAsync(CancellationToken ct = default)
        {
            // Retrieve all ideas with related data (category, user, votes, comments)
            var ideas = await ideaRepository.GetAllIdeasAsync(ct);

            // Map each idea to an IdeaResponseDto, including counts for upvotes/downvotes and comment details
            return ideas.Select(i => new IdeaResponseDto
            {
                IdeaId = i.IdeaId,
                Title = i.Title,
                Description = i.Description,
                CategoryId = i.CategoryId,
                CategoryName = i.Category.Name,
                SubmittedByUserId = i.SubmittedByUserId,
                SubmittedByUserName = i.SubmittedByUser.Name,
                SubmittedDate = i.SubmittedDate,
                Status = i.Status.ToString(),
                Upvotes = i.Votes.Count(v => v.VoteType == VoteType.Upvote),
                Downvotes = i.Votes.Count(v => v.VoteType == VoteType.Downvote),
                Comments = i.Comments.Select(c => new CommentResponseDto
                {
                    CommentId = c.CommentId,
                    UserId = c.UserId,
                    UserName = c.User.Name,
                    Text = c.Text,
                    CreatedDate = c.CreatedDate
                }).ToList()
            });
        }

        public async Task<IEnumerable<IdeaResponseDto>> GetMyIdeasAsync(Guid userId, CancellationToken ct = default)
        {
            // Retrieve ideas submitted by the specified user with related data (category, user, votes, comments)
            var ideas = await ideaRepository.GetIdeasByUserIdAsync(userId, ct);

            // Map each idea to an IdeaResponseDto, including counts for upvotes/downvotes and comment details
            return ideas.Select(i => new IdeaResponseDto
            {
                IdeaId = i.IdeaId,
                Title = i.Title,
                Description = i.Description,
                CategoryId = i.CategoryId,
                CategoryName = i.Category.Name,
                SubmittedByUserId = i.SubmittedByUserId,
                SubmittedByUserName = i.SubmittedByUser.Name,
                SubmittedDate = i.SubmittedDate,
                Status = i.Status.ToString(),
                Upvotes = i.Votes.Count(v => v.VoteType == VoteType.Upvote),
                Downvotes = i.Votes.Count(v => v.VoteType == VoteType.Downvote),
                Comments = i.Comments.Select(c => new CommentResponseDto
                {
                    CommentId = c.CommentId,
                    UserId = c.UserId,
                    UserName = c.User.Name,
                    Text = c.Text,
                    CreatedDate = c.CreatedDate
                }).ToList()
            });
        }

        public async Task<IdeaResponseDto> GetIdeaByIdAsync(Guid id, CancellationToken ct = default)
        {
            // Retrieve the idea by ID with related data (category, user, votes, comments)
            var idea = await ideaRepository.GetIdeaByIdAsync(id, ct);

            if (idea == null)
            {
                throw new KeyNotFoundException("Idea not found");
            }
            // Map the idea to an IdeaResponseDto, including counts for upvotes/downvotes and comment details
            return new IdeaResponseDto
            {
                IdeaId = idea.IdeaId,
                Title = idea.Title,
                Description = idea.Description,
                CategoryId = idea.CategoryId,
                CategoryName = idea.Category.Name,
                SubmittedByUserId = idea.SubmittedByUserId,
                SubmittedByUserName = idea.SubmittedByUser.Name,
                SubmittedDate = idea.SubmittedDate,
                Status = idea.Status.ToString(),
                Upvotes = idea.Votes.Count(v => v.VoteType == VoteType.Upvote),
                Downvotes = idea.Votes.Count(v => v.VoteType == VoteType.Downvote),
                Comments = idea.Comments.Select(c => new CommentResponseDto
                {
                    CommentId = c.CommentId,
                    UserId = c.UserId,
                    UserName = c.User.Name,
                    Text = c.Text,
                    CreatedDate = c.CreatedDate
                }).ToList()
            };
        }

        public async Task<IdeaResponseDto> SubmitIdeaAsync(Guid userId, IdeaRequestDto request, CancellationToken ct = default)
        {
            // Validate the category exists and is active
            var category = await ideaRepository.GetCategoryByIdAsync(request.CategoryId, ct);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found");
            }
            if (!category.IsActive)
            {
                throw new InvalidOperationException("Selected category is inactive");
            }
            // Validate the user exists
            var user = await ideaRepository.GetUserByIdAsync(userId, ct);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }
            // Create a new idea entity and save it to the database
            var newIdea = new Idea
            {
                IdeaId = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                SubmittedByUserId = userId,
                SubmittedDate = DateTime.Now,
                Status = IdeaStatus.UnderReview
            };
            // Save the new idea to the database
            await ideaRepository.AddAsync(newIdea, ct);
            await ideaRepository.SaveChangesAsync(ct);
            // Send a notification about the new idea submission
            await notificationService.CreateNewIdeaNotificationAsync(newIdea.IdeaId, newIdea.Title, newIdea.SubmittedByUserId);
            // Map the new idea to an IdeaResponseDto and return it
            return new IdeaResponseDto
            {
                IdeaId = newIdea.IdeaId,
                Title = newIdea.Title,
                Description = newIdea.Description,
                CategoryId = newIdea.CategoryId,
                CategoryName = category.Name,
                SubmittedByUserId = newIdea.SubmittedByUserId,
                SubmittedByUserName = user.Name,
                SubmittedDate = newIdea.SubmittedDate,
                Status = newIdea.Status.ToString(),
                Upvotes = 0,
                Downvotes = 0,
                Comments = new List<CommentResponseDto>()
            };
        }

        public async Task<IdeaResponseDto> UpdateIdeaAsync(Guid id, Guid userId, IdeaRequestDto request, CancellationToken ct = default)
        {
            // Retrieve the idea by ID and validate it exists, belongs to the user, and the category is valid and active
            var idea = await ideaRepository.GetIdeaByIdAsync(id, ct);
            if (idea == null)
            {
                throw new KeyNotFoundException("Idea not found");
            }
            // Only allow updates to ideas that belong to the user
            if (idea.SubmittedByUserId != userId)
            {
                throw new UnauthorizedAccessException("You can only update your own ideas");
            }
            
            var category = await ideaRepository.GetCategoryByIdAsync(request.CategoryId, ct);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            if (!category.IsActive)
            {
                throw new InvalidOperationException("Selected category is inactive");
            }
            // Update the idea's properties and save changes to the database
            idea.Title = request.Title;
            idea.Description = request.Description;
            idea.CategoryId = request.CategoryId;

            await ideaRepository.UpdateAsync(idea, ct);
            await ideaRepository.SaveChangesAsync(ct);
            
            var user = await ideaRepository.GetUserByIdAsync(userId, ct);
            // Map the updated idea to an IdeaResponseDto and return it
            return new IdeaResponseDto
            {
                IdeaId = idea.IdeaId,
                Title = idea.Title,
                Description = idea.Description,
                CategoryId = idea.CategoryId,
                CategoryName = category.Name,
                SubmittedByUserId = idea.SubmittedByUserId,
                SubmittedByUserName = user?.Name ?? "Unknown",
                SubmittedDate = idea.SubmittedDate,
                Status = idea.Status.ToString(),
                Upvotes = idea.Votes.Count(v => v.VoteType == VoteType.Upvote),
                Downvotes = idea.Votes.Count(v => v.VoteType == VoteType.Downvote),
                Comments = idea.Comments.Select(c => new CommentResponseDto
                {
                    CommentId = c.CommentId,
                    UserId = c.UserId,
                    UserName = c.User.Name,
                    Text = c.Text,
                    CreatedDate = c.CreatedDate
                }).ToList()
            };
        }

        public async Task DeleteIdeaAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            // Retrieve the idea by ID and validate it exists, belongs to the user, and is not approved
            var idea = await ideaRepository.GetIdeaByIdAsync(id, ct);
            if (idea == null)
            {
                throw new KeyNotFoundException("Idea not found");
            }
            // Only allow deletion of ideas that belong to the user
            if (idea.SubmittedByUserId != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own ideas");
            }
            // Prevent deletion of approved ideas
            if (idea.Status == IdeaStatus.Approved)
            {
                throw new InvalidOperationException("You cannot delete approved ideas");
            }
            // Delete the idea from the database
            await ideaRepository.DeleteAsync(idea, ct);
            await ideaRepository.SaveChangesAsync(ct);
        }
    }
}
