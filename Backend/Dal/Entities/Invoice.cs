namespace Backend.Dal.Entities
{
	public class Invoice
	{
		public int Id { get; set; }
		public string CustomerName { get; set; }
		public string CustomerEmail { get; set; }
		public string CustomerPhoneNumber { get; set; }
		public string CustomerZipCode { get; set; }
		public string CustomerCountry { get; set; }
		public string CustomerCity { get; set; }
		public string CustomerStreet { get; set; }
		public DateTime CreationDate { get; set; }
		public int OrderId { get; set; }
		public int PaymentMethodId { get; set; }
		public Order Order { get; set; }
		public PaymentMethod PaymentMethod { get; set; }
	}
}
