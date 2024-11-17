namespace Backend.Dtos.Orders.InitiatePayment;

    public class BarionPaymentStateResponse
    {
        public string PaymentId { get; set; }
        public string PaymentRequestId { get; set; }
        public string OrderNumber { get; set; }
        public string Status { get; set; }
        public string PaymentType { get; set; }
        public string FundingSource { get; set; }
        public FundingInformation FundingInformation { get; set; }
        public bool GuestCheckout { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ValidUntil { get; set; }
        public DateTime? CompletedAt { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string RedirectUrl { get; set; }
        public string CallbackUrl { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class FundingInformation
    {
        public BankCardDetails BankCard { get; set; }
        public string AuthorizationCode { get; set; }
        public string ProcessResult { get; set; }
    }

    public class BankCardDetails
    {
        public string MaskedPan { get; set; }
        public string BankCardType { get; set; }
        public int? ValidThruYear { get; set; } // Made nullable
        public int? ValidThruMonth { get; set; } // Made nullable
    }

    public class Transaction
    {
        public string TransactionId { get; set; }
        public string POSTransactionId { get; set; }
        public DateTime? TransactionTime { get; set; } // Made nullable
        public decimal Total { get; set; }
        public string Currency { get; set; }
        public Payer Payer { get; set; }
        public Payee Payee { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public string TransactionType { get; set; }
        public List<Item> Items { get; set; } = new();
    }

    public class Payer
    {
        public PayerName Name { get; set; }
        public string Email { get; set; }
    }

    public class Payee
    {
        public PayeeName Name { get; set; }
        public string Email { get; set; }
    }

    public class PayerName
    {
        public string LoginName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OrganizationName { get; set; }
    }

    public class PayeeName
    {
        public string LoginName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OrganizationName { get; set; }
    }

    public class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; } // Keep as `decimal` for precision
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ItemTotal { get; set; }
        public string SKU { get; set; }
    }

