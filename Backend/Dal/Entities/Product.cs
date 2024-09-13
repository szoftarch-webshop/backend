namespace Backend.Dal.Entities
{
	public class Product
	{
		public Product()
		{
			Categories = new HashSet<Category>();
		}

		public int Id { get; set; }
		public string Name { get; set; }
		public double Weight { get; set; }
		public string Material { get; set; }
		public string Description { get; set; }
		public int Price { get; set; }
		public int Stock { get; set; }
		public string ImageUrl { get; set; }
		public ICollection<Category> Categories { get; set; }
		public ICollection<OrderItem> OrderItems { get; set; }
	}
}
