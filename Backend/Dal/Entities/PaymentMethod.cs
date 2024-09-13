namespace Backend.Dal.Entities
{
	public class PaymentMethod
	{
		public PaymentMethod()
		{
			Orders = new HashSet<Order>();
			Invoices = new HashSet<Invoice>();
		}

		public int Id { get; set; }
		public string Name { get; set; }
		public ICollection<Order> Orders { get; set; }
		public ICollection<Invoice> Invoices { get; set; }
	}
}
