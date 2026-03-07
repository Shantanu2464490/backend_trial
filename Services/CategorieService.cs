// backend_trial/Services/CategoryService.cs
using backend_trial.Models.Domain;
using backend_trial.Models.DTO.Category;
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
            // retrun all the categories
            var entities = await categoryRepository.GetAllAsync(ct);
            return entities.Select(MapToResponse);
        }

        public async Task<CategoryResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            // Checking if category exist
            var entity = await categoryRepository.GetByIdAsync(id, ct);
            if (entity == null)
                throw new ("Category not found");

            // Retrun category if found
            return MapToResponse(entity);
        }

        public async Task<CategoryResponseDto> CreateAsync(CategoryRequestDto dto, CancellationToken ct = default)
        {
            // creating a new Category: checking is the category with the name exists
            if (await categoryRepository.ExistsByNameAsync(dto.Name, null, ct))
                throw new ("Category with this name already exists");

            // Mapping the input
            var entity = new Category
            {
                CategoryId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            // adding it to the database
            await categoryRepository.AddAsync(entity, ct);
            await categoryRepository.SaveChangesAsync(ct);

            return MapToResponse(entity);
        }

        public async Task<CategoryResponseDto> UpdateAsync(Guid id, CategoryRequestDto dto, CancellationToken ct = default)
        {
            // Fetching the category
            var existing = await categoryRepository.GetByIdAsync(id, ct);
            if (existing == null)
                throw new ("Category not found");

            // Checking if input category is valid(dublicate or not)
            if (await categoryRepository.ExistsByNameAsync(dto.Name, id, ct))
                throw new ("Another category with this name already exists");

            // mapping the new details
            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.IsActive = dto.IsActive;

            // saving it to the database
            await categoryRepository.UpdateAsync(existing, ct);
            await categoryRepository.SaveChangesAsync(ct);

            return MapToResponse(existing);
        }

        public async Task<CategoryResponseDto> ToggleStatusAsync(Guid id, CancellationToken ct = default)
        {
            // Fetching the details
            var existing = await categoryRepository.GetByIdAsync(id, ct);
            if (existing == null)
                throw new ("Category not found");

            // toggling the status
            existing.IsActive = !existing.IsActive;

            // Updating the database
            await categoryRepository.UpdateAsync(existing, ct);
            await categoryRepository.SaveChangesAsync(ct);

            return MapToResponse(existing);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            // Fetching the details
            var existing = await categoryRepository.GetByIdAsync(id, ct);
            if (existing == null)
                throw new ("Category not found");

            // Checking if it is in use
            var inUse = await categoryRepository.IsUsedByIdeasAsync(id, ct);
            if (inUse)
                throw new ("Cannot delete category as it is being used by ideas");

            // Updating the database
            await categoryRepository.DeleteAsync(existing, ct);
            await categoryRepository.SaveChangesAsync(ct);
        }

        // mapping all the details
        private static CategoryResponseDto MapToResponse(Category c) => new()
        {
            CategoryId = c.CategoryId,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive
        };
    }
}