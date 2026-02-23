using backend_trial.Models.DTO;

namespace backend_trial.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllAsync(CancellationToken ct = default);
        Task<CategoryResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<CategoryResponseDto> CreateAsync(CategoryRequestDto dto, CancellationToken ct = default);
        Task<CategoryResponseDto> UpdateAsync(Guid id, CategoryRequestDto dto, CancellationToken ct = default);
        Task<CategoryResponseDto> ToggleStatusAsync(Guid id, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}

