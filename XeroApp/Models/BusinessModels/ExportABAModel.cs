using Core;

namespace XeroApp.Models.BusinessModels
{
    public class ExportABAModel
    {
        public string BankAccountName { get; set; }

        public string BankAccountNumber { get; set; }

        public string TransactionDate { get; set; }

        public string isinsertRecordDB { get; set; }

        public List<InherittedInvoice2> Invoices { get; set; }

        public int errorCount { get; set; }
    }
}
