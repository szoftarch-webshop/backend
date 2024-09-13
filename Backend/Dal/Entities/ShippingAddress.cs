namespace Backend.Dal.Entities
{
	public class ShippingAddress
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string ZipCode { get; set; }
		public string Country { get; set; }
		public string City { get; set; }
		public string Street { get; set; }
		public Order Order { get; set; }
	}
}
