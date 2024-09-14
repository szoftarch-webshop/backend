namespace Backend.Dtos.Orders;

public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerPhoneNumber { get; set; }
    public string CustomerZipCode { get; set; }
    public string CustomerCountry { get; set; }
    public string CustomerCity { get; set; }
    public string CustomerStreet { get; set; }
    public DateTime CreationDate { get; set; }
        
    // Optional: include order details
    public int OrderId { get; set; }

    // Optional: include payment method details
    public int PaymentMethodId { get; set; }
    public PaymentMethodDto PaymentMethod { get; set; }

}