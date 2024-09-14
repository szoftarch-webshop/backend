namespace Backend.Dtos.Products
{
	public class CreateProductDto
	{
		public string SerialNumber { get; set; }
		public string Name { get; set; }
		public double Weight { get; set; }
		public string Material { get; set; }
		public string Description { get; set; }
		public int Price { get; set; }
		public int Stock { get; set; }
		public string ImageUrl { get; set; }
		public List<string> CategoryNames { get; set; }
	}
}
