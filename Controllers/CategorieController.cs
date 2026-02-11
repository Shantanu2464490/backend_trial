using backend_trial.Data;
using backend_trial.Models.Domain;
using backend_trial.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_trial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategorieController : ControllerBase
    {
        private readonly IdeaBoardDbContext _dbContext;

        public CategorieController(IdeaBoardDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Get all categories
        [HttpGet("categories")]
        public async Task<ActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _dbContext.Categories.ToListAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error retrieving categories", Error = ex.Message });
            }
        }

        // Get category by ID
        [HttpGet("categories/{id}")]
        public async Task<ActionResult> GetCategoryById(Guid id)
        {
            try
            {
                var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
                if (category == null)
                {
                    return NotFound(new { Message = "Category not found" });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error retrieving category", Error = ex.Message });
            }
        }

        // Add new category
        [HttpPost("categories")]
        public async Task<ActionResult> AddCategory([FromBody] CategoryRequestDto categoryRequestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if category name already exists
                var existingCategory = await _dbContext.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryRequestDto.Name.ToLower());

                if (existingCategory != null)
                {
                    return Conflict(new { Message = "Category with this name already exists" });
                }

                var newCategory = new Category
                {
                    CategoryId = Guid.NewGuid(),
                    Name = categoryRequestDto.Name,
                    Description = categoryRequestDto.Description,
                    IsActive = categoryRequestDto.IsActive
                };

                _dbContext.Categories.Add(newCategory);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.CategoryId }, newCategory);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error adding category", Error = ex.Message });
            }
        }

        // Update category
        [HttpPut("categories/{id}")]
        public async Task<ActionResult> UpdateCategory(Guid id, [FromBody] CategoryRequestDto categoryRequestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
                if (category == null)
                {
                    return NotFound(new { Message = "Category not found" });
                }

                // Check if another category has the same name
                var existingCategory = await _dbContext.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryRequestDto.Name.ToLower() && c.CategoryId != id);

                if (existingCategory != null)
                {
                    return Conflict(new { Message = "Another category with this name already exists" });
                }

                category.Name = categoryRequestDto.Name;
                category.Description = categoryRequestDto.Description;
                category.IsActive = categoryRequestDto.IsActive;

                _dbContext.Categories.Update(category);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = "Category updated successfully", Category = category });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error updating category", Error = ex.Message });
            }
        }

        // Toggle category active/inactive status
        [HttpPatch("categories/{id}/toggle-status")]
        public async Task<ActionResult> ToggleCategoryStatus(Guid id)
        {
            try
            {
                var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
                if (category == null)
                {
                    return NotFound(new { Message = "Category not found" });
                }

                category.IsActive = !category.IsActive;
                _dbContext.Categories.Update(category);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = $"Category is now {(category.IsActive ? "active" : "inactive")}", Category = category });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error toggling category status", Error = ex.Message });
            }
        }

        // Delete category
        [HttpDelete("categories/{id}")]
        public async Task<ActionResult> DeleteCategory(Guid id)
        {
            try
            {
                var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
                if (category == null)
                {
                    return NotFound(new { Message = "Category not found" });
                }

                // Check if category is being used by any ideas
                var ideasUsingCategory = await _dbContext.Ideas
                    .AnyAsync(i => i.CategoryId == id);

                if (ideasUsingCategory)
                {
                    return BadRequest(new { Message = "Cannot delete category as it is being used by ideas" });
                }

                _dbContext.Categories.Remove(category);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error deleting category", Error = ex.Message });
            }
        }
    }
}
