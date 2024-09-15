using Backend.Dal.Context;

namespace Backend.Services
{
	public class CategoryService
	{
		private readonly DataContext _context;

		public CategoryService(DataContext context)
		{
			_context = context;
		}

		public List<int> GetAllDescendantCategoryIds(int categoryId)
		{
			var categoryIds = new HashSet<int>();
			var categoriesToProcess = new Queue<int>();

			categoriesToProcess.Enqueue(categoryId);

			while (categoriesToProcess.Count > 0)
			{
				var currentCategoryId = categoriesToProcess.Dequeue();
				categoryIds.Add(currentCategoryId);

				var childCategories = _context.Category
					.Where(c => c.ParentCategoryId == currentCategoryId)
					.Select(c => c.Id)
					.ToList();

				foreach (var childCategoryId in childCategories)
				{
					categoriesToProcess.Enqueue(childCategoryId);
				}
			}

			return categoryIds.ToList();
		}
	}
}
