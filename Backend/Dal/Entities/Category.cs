namespace Backend.Dal.Entities
{
	public class Category
	{
		public Category()
		{
			ChildrenCategories = new HashSet<Category>();
			Products = new HashSet<Product>();
		}

		public int Id { get; set; }
		public string Name { get; set; }
		public int? ParentCategoryId { get; set; }

		public Category ParentCategory { get; set; }
		public ICollection<Category> ChildrenCategories { get; set; }
		public ICollection<Product> Products { get; set; }
	}
}
