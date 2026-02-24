using backend_trial.Models.DTO;

namespace backend_trial.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<IdeaWithDetailsResponseDto>> GetAllIdeasForReviewAsync(CancellationToken ct);
        Task<IEnumerable<IdeaWithDetailsResponseDto>> GetIdeasByStatusAsync(string status, CancellationToken ct);
        Task<IdeaWithDetailsResponseDto> GetIdeaForReviewAsync(Guid ideaId, CancellationToken ct);

        Task<ReviewResponseDto> SubmitFeedbackAsync(Guid ideaId, Guid managerId, string feedback, CancellationToken ct);

        Task<ReviewResponseDto> GetReviewByIdAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<ReviewResponseDto>> GetReviewsForIdeaAsync(Guid ideaId, CancellationToken ct);
        Task<IEnumerable<ReviewResponseDto>> GetMyReviewsAsync(Guid managerId, CancellationToken ct);

        Task ChangeIdeaStatusAsync(Guid ideaId, string newStatus, string? reviewComment, Guid managerId, CancellationToken ct);
    }
}