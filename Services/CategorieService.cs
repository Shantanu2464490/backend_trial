// backend_trial/Services/CategoryService.cs
using backend_trial.Models.Domain;
using backend_trial.Models.DTO;
using backend_trial.Repositories.Interfaces;
using backend_trial.Services.Interfaces;

namespace backend_trial.Services
{
    public class CategorieService : ICategoryService
    {
        private readonly ICategoryRepository categoryRepository;

        public CategorieService(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync(CancellationToken ct = default)
        {
            var entities = await categoryRepository.GetAllAsync(ct);
            return entities.Select(MapToResponse);
        }

        public async Task<CategoryResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await categoryRepository.GetByIdAsync(id, ct);
            if (entity == null)
                throw new ("Category not found");

            return MapToResponse(entity);
        }

        public async Task<CategoryResponseDto> CreateAsync(CategoryRequestDto dto, CancellationToken ct = default)
        {
            if (await categoryRepository.ExistsByNameAsync(dto.Name, null, ct))
                throw new ("Category with this name already exists");

            var entity = new Category
            {
                CategoryId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            await categoryRepository.AddAsync(entity, ct);
            await categoryRepository.SaveChangesAsync(ct);

            return MapToResponse(entity);
        }

        public async Task<CategoryResponseDto> UpdateAsync(Guid id, CategoryRequestDto dto, CancellationToken ct = default)
        {
            var existing = await categoryRepository.GetByIdAsync(id, ct);
            if (existing == null)
                throw new ("Category not found");

            if (await categoryRepository.ExistsByNameAsync(dto.Name, id, ct))
                throw new ("Another category with this name already exists");

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.IsActive = dto.IsActive;

            await categoryRepository.UpdateAsync(existing, ct);
            await categoryRepository.SaveChangesAsync(ct);

            return MapToResponse(existing);
        }

        public async Task<CategoryResponseDto> ToggleStatusAsync(Guid id, CancellationToken ct = default)
        {
            var existing = await categoryRepository.GetByIdAsync(id, ct);
            if (existing == null)
                throw new ("Category not found");

            existing.IsActive = !existing.IsActive;

            await categoryRepository.UpdateAsync(existing, ct);
            await categoryRepository.SaveChangesAsync(ct);

            return MapToResponse(existing);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var existing = await categoryRepository.GetByIdAsync(id, ct);
            if (existing == null)
                throw new ("Category not found");

            var inUse = await categoryRepository.IsUsedByIdeasAsync(id, ct);
            if (inUse)
                throw new ("Cannot delete category as it is being used by ideas");

            await categoryRepository.DeleteAsync(existing, ct);
            await categoryRepository.SaveChangesAsync(ct);
        }

        private static CategoryResponseDto MapToResponse(Category c) => new()
        {
            CategoryId = c.CategoryId,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive
        };
    }
}