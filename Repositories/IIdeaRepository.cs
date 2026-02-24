using backend_trial.Models.Domain;

namespace backend_trial.Repositories.Interfaces
{
    public interface IIdeaRepository
    {
        Task<IEnumerable<Idea>> GetAllIdeasAsync(CancellationToken ct = default);
        Task<IEnumerable<Idea>> GetIdeasByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<Idea?> GetIdeaByIdAsync(Guid id, CancellationToken ct = default);
        Task<Category?> GetCategoryByIdAsync(Guid categoryId, CancellationToken ct = default);
        Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
        Task AddAsync(Idea idea, CancellationToken ct = default);
        Task UpdateAsync(Idea idea, CancellationToken ct = default);
        Task DeleteAsync(Idea idea, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
