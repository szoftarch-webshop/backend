namespace Backend.Dtos.Orders;

public class InvoiceDto
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
    public PaymentMethodDto PaymentMethod { get; set; }

}