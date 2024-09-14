using Backend.Dal.Interfaces;
using Backend.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _categoryRepository.GetCategoriesAsync();
            return Ok(categories);
        }

        // POST: api/Category
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto? createCategoryDto)
        {
            if (createCategoryDto == null || string.IsNullOrWhiteSpace(createCategoryDto.Name))
            {
                return BadRequest("Category name is required.");
            }

            var createdCategory = await _categoryRepository.CreateCategoryAsync(createCategoryDto);
            return CreatedAtAction(nameof(GetCategories), new { id = createdCategory.Id }, createdCategory);
        }

        // PUT: api/Category/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> RenameCategory(int id, [FromBody] RenameCategoryDto? renameCategoryDto)
        {
            if (renameCategoryDto == null || string.IsNullOrWhiteSpace(renameCategoryDto.NewName))
            {
                return BadRequest("New category name is required.");
            }

            var success = await _categoryRepository.RenameCategoryAsync(id, renameCategoryDto);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Category/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var success = await _categoryRepository.DeleteCategoryAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
