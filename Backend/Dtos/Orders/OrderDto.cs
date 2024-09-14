namespace Backend.Dtos.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public string Status { get; set; }
    public DateTime OrderDate { get; set; }
    public ICollection<OrderItemDto> OrderItems { get; set; }
    public ShippingAddressDto ShippingAddress { get; set; }
    public PaymentMethodDto PaymentMethod { get; set; }
    public InvoiceDto Invoice { get; set; }
}