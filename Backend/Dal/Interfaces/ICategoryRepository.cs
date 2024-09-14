using Backend.Dtos;

namespace Backend.Dal.Interfaces
{
	public interface ICategoryRepository
	{
		// GET: Retrieve all categories including parent-child hierarchy
		Task<IEnumerable<CategoryDto>> GetCategoriesAsync();

		// POST: Create a new category using a DTO
		Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto? createCategoryDto);

		// PUT: Update an existing category using a DTO
		Task<bool> UpdateCategoryAsync(int categoryId, CreateCategoryDto? renameCategoryDto);

		// DELETE: Delete an existing category by ID
		Task<bool> DeleteCategoryAsync(int categoryId);
	}
}
