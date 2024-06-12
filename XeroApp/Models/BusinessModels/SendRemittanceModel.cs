
using Core;
using Xero.NetStandard.OAuth2.Model.Accounting;

namespace XeroApp.Models.BusinessModels
{
    public class SendRemittanceModel
    {

        public List<InherittedInvoice2> selectedInvoices { get; set; }

        public Payments payments { get; set; }
    }
}
