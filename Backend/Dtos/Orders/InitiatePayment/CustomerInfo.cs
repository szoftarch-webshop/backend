namespace Backend.Dtos.Orders.InitiatePayment;

public class CustomerInfo
{
    public string Name { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string PhoneNumber { get; set; }
    public string EmailAddress { get; set; }
}