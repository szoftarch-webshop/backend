using Backend.Dtos;

namespace Backend.Dal.Interfaces
{
	public interface ICategoryRepository
	{
		// GET: Retrieve all categories including parent-child hierarchy
		Task<IEnumerable<CategoryDto>> GetCategoriesAsync();

		// POST: Create a new category using a DTO
		Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto? createCategoryDto);

		// PUT: Rename an existing category using a DTO
		Task<bool> RenameCategoryAsync(int categoryId, RenameCategoryDto? renameCategoryDto);

		// DELETE: Delete an existing category by ID
		Task<bool> DeleteCategoryAsync(int categoryId);
	}
}
