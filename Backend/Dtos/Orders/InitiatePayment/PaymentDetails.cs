namespace Backend.Dtos.Orders.InitiatePayment;

public class PaymentDetails
{
    public CustomerInfo CustomerInfo { get; set; }
    public List<CartItem> CartItems { get; set; }
    public int TotalAmount { get; set; }
}