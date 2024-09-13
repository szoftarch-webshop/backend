namespace Backend.Dal.Entities
{
	public class OrderItem
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public int Amount { get; set; }
		public int OrderedPrice { get; set; }
		public int OrderId { get; set; }
		public Order Order { get; set; }
		public Product Product { get; set; }
	}
}
