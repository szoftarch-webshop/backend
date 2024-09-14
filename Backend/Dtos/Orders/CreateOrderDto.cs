using Backend.Dal.Entities;

namespace Backend.Dtos.Orders;

public class CreateOrderDto
{
    public DateTime OrderDate { get; set; }
    public CreateShippingAddress ShippingAddress { get; set; }
    public CreateInvoiceDto Invoice { get; set; }
    public ICollection<CreateOrderItemDto> OrderItems { get; set; }
}