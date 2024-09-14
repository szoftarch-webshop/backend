using Backend.Dal.Context;
using Backend.Dal.Entities;
using Backend.Dal.Interfaces;
using Backend.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Backend.Dal.Repositories
{
	public class CategoryRepository : ICategoryRepository
	{
		private readonly DataContext _context;

        public CategoryRepository(DataContext context)
        {
            _context = context;
        }

        // GET: Retrieve all categories including parent-child hierarchy
        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            var categories = await _context.Category
                .Include(c => c.ChildrenCategories) // Assuming you have a navigation property for child categories
                .ToListAsync();

            return categories.Select(MapToCategoryDto).ToList();
        }

        // POST: Create a new category
        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto? createCategoryDto)
        {
            var category = new Category
            {
                Name = createCategoryDto.Name,
                ParentCategoryId = createCategoryDto.ParentId
            };

            _context.Category.Add(category);
            await _context.SaveChangesAsync();

            return MapToCategoryDto(category);
        }

        // PUT: Rename an existing category
        public async Task<bool> RenameCategoryAsync(int categoryId, RenameCategoryDto? renameCategoryDto)
        {
            var category = await _context.Category.FindAsync(categoryId);
            if (category == null)
            {
                return false;
            }

            category.Name = renameCategoryDto.NewName;
            await _context.SaveChangesAsync();

            return true;
        }

        // DELETE: Delete an existing category by ID
        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var category = await _context.Category.FindAsync(categoryId);
            if (category == null)
            {
                return false;
            }

            _context.Category.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }

        // Helper method to map Category entity to CategoryDto
        private CategoryDto MapToCategoryDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentCategoryId,
                Children = category.ChildrenCategories?.Select(MapToCategoryDto).ToList()
            };
        }
	}
}
