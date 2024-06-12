using Core;

namespace XeroApp.Models.BusinessModels
{
    public class BillsPaymentModel
    {
        public string BankAccountNumber { get; set; }
        public string BankAccountCode { get; set; }
        public string TransactionDate { get; set; }
        public List<InherittedInvoice2> Invoices { get; set; }
    }
}