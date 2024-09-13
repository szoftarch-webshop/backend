namespace Backend.Dal.Entities
{
	public class Order
	{
		public Order()
		{
			OrderItems = new HashSet<OrderItem>();
		}

		public int Id { get; set; }
		public string Status { get; set; }
		public DateTime OrderDate { get; set; }
		public ICollection<OrderItem> OrderItems { get; set; }
		public int ShippingAddressId { get; set; }
		public int InvoiceId { get; set; }
		public ShippingAddress ShippingAddress { get; set; }
		public Invoice Invoice { get; set; }
	}
}
