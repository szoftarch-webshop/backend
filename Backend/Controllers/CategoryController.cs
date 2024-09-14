using Backend.Dal.Interfaces;
using Backend.Dtos;
using Microsoft.AspNetCore.Authorization;
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
            try
            {
                var createdCategory = await _categoryRepository.CreateCategoryAsync(createCategoryDto);
                return CreatedAtAction(nameof(GetCategories), new { id = createdCategory.Id }, createdCategory);
            } catch (Exception ex)
            {
				return BadRequest(ex.Message);
			}
        }

        // PUT: api/Category/{id}
        [HttpPut("{id}")]
		public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDto? updatedCategoryDto)
        {
            try
            {
                var success = await _categoryRepository.UpdateCategoryAsync(id, updatedCategoryDto);
                return success ? NoContent() : NotFound();
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/Category/{id}
        [HttpDelete("{id}")]
		public async Task<IActionResult> DeleteCategory(int id)
        {
            var success = await _categoryRepository.DeleteCategoryAsync(id);
			return success ? NoContent() : NotFound();
		}
    }
}
