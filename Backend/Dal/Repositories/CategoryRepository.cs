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
			if (createCategoryDto == null || string.IsNullOrWhiteSpace(createCategoryDto.Name))
			{
				throw new Exception("Category name is required.");
			}

			var existingCategory = await _context.Category.FirstOrDefaultAsync(c => c.Name == createCategoryDto.Name);
			if (existingCategory != null)
			{
				throw new Exception("Category with the same name already exists.");
			}

            if (createCategoryDto.ParentId.HasValue)
            {
				var parentCategory = await _context.Category.FindAsync(createCategoryDto.ParentId.Value);
				if (parentCategory == null)
                {
					throw new Exception("Parent category does not exist.");
				}
			}

			var category = new Category
			{
				Name = createCategoryDto.Name,
				ParentCategoryId = createCategoryDto.ParentId
			};

			_context.Category.Add(category);
			await _context.SaveChangesAsync();

			return MapToCategoryDto(category);
		}

        // PUT: Update an existing category
        public async Task<bool> UpdateCategoryAsync(int categoryId, CreateCategoryDto? updatedCategoryDto)
        {
			if (updatedCategoryDto == null || string.IsNullOrWhiteSpace(updatedCategoryDto.Name))
			{
				throw new Exception("New category name is required.");
			}

			var existingCategory = await _context.Category.SingleOrDefaultAsync(c => c.Name == updatedCategoryDto.Name && c.Id != categoryId);
			if (existingCategory != null)
			{
				throw new Exception("Category with the same name already exists.");
			}

			var category = await _context.Category.FindAsync(categoryId);
            if (category == null)
            {
                return false;
            }

			if (updatedCategoryDto.ParentId.HasValue)
			{
				var parentCategory = await _context.Category.FindAsync(updatedCategoryDto.ParentId.Value);
				if (parentCategory == null)
				{
					throw new Exception("Parent category does not exist.");
				}
                if(parentCategory.Id == category.Id)
                {
					throw new Exception("Category cannot be its own parent.");
				}
			}

			category.Name = updatedCategoryDto.Name;
            category.ParentCategoryId = updatedCategoryDto.ParentId;
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
