using backend_trial.Models.Domain;
using backend_trial.Models.DTO;
using backend_trial.Repositories.Interfaces;
using backend_trial.Services.Interfaces;

namespace backend_trial.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            this.commentRepository = commentRepository;
        }

        public async Task<CommentResponseDto> AddCommentAsync(Guid ideaId, Guid userId, CommentRequestDto request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                throw new ArgumentException("Comment text cannot be empty");
            }

            var ideaExists = await commentRepository.IdeaExistsAsync(ideaId, ct);
            if (!ideaExists)
            {
                throw new KeyNotFoundException("Idea not found");
            }

            var user = await commentRepository.GetUserByIdAsync(userId, ct);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            var comment = new Comment
            {
                CommentId = Guid.NewGuid(),
                IdeaId = ideaId,
                UserId = userId,
                Text = request.Text,
                CreatedDate = DateTime.UtcNow
            };

            await commentRepository.AddAsync(comment, ct);
            await commentRepository.SaveChangesAsync(ct);

            return new CommentResponseDto
            {
                CommentId = comment.CommentId,
                UserId = comment.UserId,
                UserName = user.Name,
                Text = comment.Text,
                CreatedDate = comment.CreatedDate
            };
        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsByIdeaIdAsync(Guid ideaId, CancellationToken ct = default)
        {
            var ideaExists = await commentRepository.IdeaExistsAsync(ideaId, ct);
            if (!ideaExists)
            {
                throw new KeyNotFoundException("Idea not found");
            }

            var comments = await commentRepository.GetCommentsByIdeaIdAsync(ideaId, ct);

            return comments.Select(c => new CommentResponseDto
            {
                CommentId = c.CommentId,
                UserId = c.UserId,
                UserName = c.User.Name,
                Text = c.Text,
                CreatedDate = c.CreatedDate
            });
        }

        public async Task<CommentResponseDto> GetCommentByIdAsync(Guid id, CancellationToken ct = default)
        {
            var comment = await commentRepository.GetByIdAsync(id, ct);

            if (comment == null)
            {
                throw new KeyNotFoundException("Comment not found");
            }

            return new CommentResponseDto
            {
                CommentId = comment.CommentId,
                UserId = comment.UserId,
                UserName = comment.User.Name,
                Text = comment.Text,
                CreatedDate = comment.CreatedDate
            };
        }

        public async Task<CommentResponseDto> UpdateCommentAsync(Guid id, Guid userId, CommentRequestDto request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                throw new ArgumentException("Comment text cannot be empty");
            }

            var comment = await commentRepository.GetByIdAsync(id, ct);
            if (comment == null)
            {
                throw new KeyNotFoundException("Comment not found");
            }

            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only update your own comments");
            }

            comment.Text = request.Text;
            await commentRepository.UpdateAsync(comment, ct);
            await commentRepository.SaveChangesAsync(ct);

            var user = await commentRepository.GetUserByIdAsync(userId, ct);

            return new CommentResponseDto
            {
                CommentId = comment.CommentId,
                UserId = comment.UserId,
                UserName = user?.Name ?? "Unknown",
                Text = comment.Text,
                CreatedDate = comment.CreatedDate
            };
        }

        public async Task DeleteCommentAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            var comment = await commentRepository.GetByIdAsync(id, ct);
            if (comment == null)
            {
                throw new KeyNotFoundException("Comment not found");
            }

            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own comments");
            }

            await commentRepository.DeleteAsync(comment, ct);
            await commentRepository.SaveChangesAsync(ct);
        }
    }
}
