namespace Backend.Dtos.Orders.InitiatePayment;

public class BarionPaymentResponse
{
    public string PaymentId { get; set; }
    public string PaymentRequestId { get; set; }
    public string Status { get; set; }
    public string QRUrl { get; set; }
    public string GatewayUrl { get; set; }
    public string RedirectUrl { get; set; }
    public List<Transaction> Transactions { get; set; }

    public class Transaction
    {
        public string POSTransactionId { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public string Currency { get; set; }
        public DateTime TransactionTime { get; set; }
    }
}
