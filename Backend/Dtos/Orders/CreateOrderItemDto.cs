namespace Backend.Dtos.Orders;

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Amount { get; set; }
    public int OrderedPrice { get; set; }
}