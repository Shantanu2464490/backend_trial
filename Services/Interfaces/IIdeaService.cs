using backend_trial.Models.DTO.Idea;

namespace backend_trial.Services.Interfaces
{
    public interface IIdeaService
    {
        Task<IEnumerable<IdeaResponseDto>> GetAllIdeasAsync(CancellationToken ct = default);
        Task<IEnumerable<IdeaResponseDto>> GetMyIdeasAsync(Guid userId, CancellationToken ct = default);
        Task<IdeaResponseDto> GetIdeaByIdAsync(Guid id, CancellationToken ct = default);
        Task<IdeaResponseDto> SubmitIdeaAsync(Guid userId, IdeaRequestDto request, CancellationToken ct = default);
        Task<IdeaResponseDto> UpdateIdeaAsync(Guid id, Guid userId, IdeaRequestDto request, CancellationToken ct = default);
        Task DeleteIdeaAsync(Guid id, Guid userId, CancellationToken ct = default);
    }
}
