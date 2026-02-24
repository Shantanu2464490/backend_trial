using backend_trial.Models.DTO.Comment;

namespace backend_trial.Services.Interfaces
{
    public interface ICommentService
    {
        Task<CommentResponseDto> AddCommentAsync(Guid ideaId, Guid userId, CommentRequestDto request, CancellationToken ct = default);
        Task<IEnumerable<CommentResponseDto>> GetCommentsByIdeaIdAsync(Guid ideaId, CancellationToken ct = default);
        Task<CommentResponseDto> GetCommentByIdAsync(Guid id, CancellationToken ct = default);
        Task<CommentResponseDto> UpdateCommentAsync(Guid id, Guid userId, CommentRequestDto request, CancellationToken ct = default);
        Task DeleteCommentAsync(Guid id, Guid userId, CancellationToken ct = default);
    }
}
