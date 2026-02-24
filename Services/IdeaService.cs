using backend_trial.Models.Domain;
using backend_trial.Models.DTO;
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
            var ideas = await ideaRepository.GetAllIdeasAsync(ct);

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
            var ideas = await ideaRepository.GetIdeasByUserIdAsync(userId, ct);

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
            var idea = await ideaRepository.GetIdeaByIdAsync(id, ct);

            if (idea == null)
            {
                throw new KeyNotFoundException("Idea not found");
            }

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
            var category = await ideaRepository.GetCategoryByIdAsync(request.CategoryId, ct);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            if (!category.IsActive)
            {
                throw new InvalidOperationException("Selected category is inactive");
            }

            var user = await ideaRepository.GetUserByIdAsync(userId, ct);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            var newIdea = new Idea
            {
                IdeaId = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                SubmittedByUserId = userId,
                SubmittedDate = DateTime.UtcNow,
                Status = IdeaStatus.UnderReview
            };

            await ideaRepository.AddAsync(newIdea, ct);
            await ideaRepository.SaveChangesAsync(ct);

            await notificationService.CreateNewIdeaNotificationAsync(newIdea.IdeaId, newIdea.Title, newIdea.SubmittedByUserId);

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
            var idea = await ideaRepository.GetIdeaByIdAsync(id, ct);
            if (idea == null)
            {
                throw new KeyNotFoundException("Idea not found");
            }

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

            idea.Title = request.Title;
            idea.Description = request.Description;
            idea.CategoryId = request.CategoryId;

            await ideaRepository.UpdateAsync(idea, ct);
            await ideaRepository.SaveChangesAsync(ct);

            var user = await ideaRepository.GetUserByIdAsync(userId, ct);

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
            var idea = await ideaRepository.GetIdeaByIdAsync(id, ct);
            if (idea == null)
            {
                throw new KeyNotFoundException("Idea not found");
            }

            if (idea.SubmittedByUserId != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own ideas");
            }

            if (idea.Status != IdeaStatus.Rejected)
            {
                throw new InvalidOperationException("You can only delete ideas in Draft status");
            }

            await ideaRepository.DeleteAsync(idea, ct);
            await ideaRepository.SaveChangesAsync(ct);
        }
    }
}
