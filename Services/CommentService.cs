using backend_trial.Models.Domain;
using backend_trial.Models.DTO.Comment;
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
            // checking the input valid or not
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                throw new ArgumentException("Comment text cannot be empty");
            }
            // checking is ideas exists or not
            var ideaExists = await commentRepository.IdeaExistsAsync(ideaId, ct);
            if (!ideaExists)
            {
                throw new KeyNotFoundException("Idea not found");
            }
            // checking is user exists or not
            var user = await commentRepository.GetUserByIdAsync(userId, ct);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            // mapping the comment details
            var comment = new Comment
            {
                CommentId = Guid.NewGuid(),
                IdeaId = ideaId,
                UserId = userId,
                Text = request.Text,
                CreatedDate = DateTime.UtcNow
            };

            // update database
            await commentRepository.AddAsync(comment, ct);
            await commentRepository.SaveChangesAsync(ct);

            // mapping the response
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
            // checking is ideas exists or not
            var ideaExists = await commentRepository.IdeaExistsAsync(ideaId, ct);
            if (!ideaExists)
            {
                throw new KeyNotFoundException("Idea not found");
            }

            // Fetching data from the database
            var comments = await commentRepository.GetCommentsByIdeaIdAsync(ideaId, ct);

            // mapping the response
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
            // Fetching data from the database
            var comment = await commentRepository.GetByIdAsync(id, ct);

            if (comment == null)
            {
                throw new KeyNotFoundException("Comment not found");
            }
            // mapping the response
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
            // checking the input valid or not
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                throw new ArgumentException("Comment text cannot be empty");
            }
            // Fetching data from the database
            var comment = await commentRepository.GetByIdAsync(id, ct);
            if (comment == null)
            {
                throw new KeyNotFoundException("Comment not found");
            }
            // checking is user exists or not
            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only update your own comments");
            }
            // mapping the comment details
            comment.Text = request.Text;
            await commentRepository.UpdateAsync(comment, ct);
            await commentRepository.SaveChangesAsync(ct);
            // fetching user details for response
            var user = await commentRepository.GetUserByIdAsync(userId, ct);
            // mapping the response
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
            // Fetching data from the database
            var comment = await commentRepository.GetByIdAsync(id, ct);
            if (comment == null)
            {
                throw new KeyNotFoundException("Comment not found");
            }
            // checking is user exists or not
            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own comments");
            }
            // update database
            await commentRepository.DeleteAsync(comment, ct);
            await commentRepository.SaveChangesAsync(ct);
        }
    }
}
