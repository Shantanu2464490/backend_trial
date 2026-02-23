using backend_trial.Data;
using backend_trial.Models.Domain;
using backend_trial.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_trial.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IdeaBoardDbContext context;

        public CategoryRepository(IdeaBoardDbContext context)
        {
            this.context = context;
        }

        public async Task<List<Category>> GetAllAsync(CancellationToken ct = default)
        {
            return await context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(ct);
        }

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == id, ct);
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludingId = null, CancellationToken ct = default)
        {
            name = name.ToLowerInvariant();
            return await context.Categories.AnyAsync(
                c => c.Name.ToLower() == name && (!excludingId.HasValue || c.CategoryId != excludingId.Value),
                ct
            );
        }

        public async Task<bool> IsUsedByIdeasAsync(Guid categoryId, CancellationToken ct = default)
        {
            return await context.Ideas.AnyAsync(i => i.CategoryId == categoryId, ct);
        }

        public async Task AddAsync(Category category, CancellationToken ct = default)
        {
            await context.Categories.AddAsync(category, ct);
        }

        public Task UpdateAsync(Category category, CancellationToken ct = default)
        {
            context.Categories.Update(category);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Category category, CancellationToken ct = default)
        {
            context.Categories.Remove(category);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            return context.SaveChangesAsync(ct);
        }
    }
}