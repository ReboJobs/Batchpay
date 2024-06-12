using Xero.NetStandard.OAuth2.Model.Accounting;

namespace Core
{
    public class InherittedInvoice : Invoice
    {
        public decimal? AmtDue { get; set; }
        public decimal? AmtPaid { get; set; }
        public string? ContactName { get; set; }
	}

    public class InherittedInvoice2
    {
		public Guid InvoiceID { get; set; }
		public decimal? AmountDueEditable { get; set; }
        public decimal? AmountDue { get; set; }
        public decimal? AmountPaid { get; set; }
		public Contact? Contact { get; set; }
		public List<LineItem>? LineItems { get; set; }
		public string? InvoiceNumber { get; set; }
        public string? Reference { get; set; }
        public string? Date { get; set; }
        public string? DueDate { get; set; }
        public string Status { get; set; }
        public bool? SentToContact { get; set; }
        public string? ContactName { get; set; }
        public string PaymentDate { get; set; }
        public string SendDate { get; set; }
        public string EmailAddress { get; set; }
        public bool HasEmailAddress { get; set; } = false;

    }
}