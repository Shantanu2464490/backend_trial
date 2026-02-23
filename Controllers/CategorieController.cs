// backend_trial/Controllers/CategorieController.cs
using backend_trial.Models.DTO;
using backend_trial.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_trial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CategorieController : ControllerBase
    {
        private readonly ICategoryService categoryService;

       
        public CategorieController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }

        // GET: api/Categorie/categories
        [HttpGet("categories")]

        public async Task<ActionResult> GetAllCategories(CancellationToken ct)
        {
            var result = await categoryService.GetAllAsync(ct);
            return Ok(result);
        }

        // GET: api/Categorie/categories/{id}
        [HttpGet("categories/{id:guid}")]
        public async Task<ActionResult> GetCategoryById(Guid id, CancellationToken ct)
        {
            var category = await categoryService.GetByIdAsync(id, ct);
            return Ok(category);
        }

        // POST: api/Categorie/categories
        [HttpPost("categories")]
        public async Task<ActionResult> AddCategory([FromBody] CategoryRequestDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await categoryService.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetCategoryById), new { id = created.CategoryId }, created);
        }

        // PUT: api/Categorie/categories/{id}
        [HttpPut("categories/{id:guid}")]
        public async Task<ActionResult> UpdateCategory(Guid id, [FromBody] CategoryRequestDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await categoryService.UpdateAsync(id, dto, ct);
            return Ok(new { Message = "Category updated successfully", Category = updated });
        }

        // PATCH: api/Categorie/categories/{id}/toggle-status
        [HttpPatch("categories/{id:guid}/toggle-status")]
        public async Task<ActionResult> ToggleCategoryStatus(Guid id, CancellationToken ct)
        {
            var updated = await categoryService.ToggleStatusAsync(id, ct);
            return Ok(new
            {
                Message = $"Category is now {(updated.IsActive ? "active" : "inactive")}",
                Category = updated
            });
        }

        // DELETE: api/Categorie/categories/{id}
        [HttpDelete("categories/{id:guid}")]
        public async Task<ActionResult> DeleteCategory(Guid id, CancellationToken ct)
        {
            await categoryService.DeleteAsync(id, ct);
            return Ok(new { Message = "Category deleted successfully" });
        }
    }
}