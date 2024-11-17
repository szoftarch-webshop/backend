namespace Backend.Dtos.Orders.InitiatePayment;

public class PaymentResponse
{
    public bool IsSuccessful { get; set; }
    public string? PaymentUrl { get; set; }
    public string? ErrorBody { get; set; }
}