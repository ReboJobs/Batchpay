using Core;
using Xero.NetStandard.OAuth2.Model.Accounting;

namespace XeroApp.Models.BusinessModels
{
    public class RemittanceAdviceModel
    {
        public Guid? ContactId { get; set; }
        public string MyCompanyName { get; set; } = string.Empty;
        public string MyCompanyAddress1 { get; set; } = string.Empty;
        public string MyCompanyAddress2 { get; set; } = string.Empty;

        public string MyCompanyAddress3 { get; set; } = string.Empty;

        public string MyCompanyCity { get; set; } = string.Empty;

        public string ContactEmailAddress { get; set; } = string.Empty;

        public string ContactName { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public decimal StillOwning { get; set; }


        public string PostalCode { get; set; } = string.Empty;
        public Contact Contact { get; set; }
        public List<InherittedInvoice2> Invoices { get; set; }
    }
}