
using backend_trial.Models.Domain;

namespace backend_trial.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync(CancellationToken ct = default);
        Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string name, Guid? excludingId = null, CancellationToken ct = default);
        Task<bool> IsUsedByIdeasAsync(Guid categoryId, CancellationToken ct = default);
        Task AddAsync(Category category, CancellationToken ct = default);
        Task UpdateAsync(Category category, CancellationToken ct = default);
        Task DeleteAsync(Category category, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}