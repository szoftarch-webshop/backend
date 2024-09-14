namespace Backend.Dtos.Orders;

public class CreateOrderDto
{
    public DateTime OrderDate { get; set; }
    public int ShippingAddressId { get; set; }
    public ICollection<CreateOrderItemDto> OrderItems { get; set; }
}