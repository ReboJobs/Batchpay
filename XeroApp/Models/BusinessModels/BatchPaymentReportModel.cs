using Core;
using Xero.NetStandard.OAuth2.Model.Accounting;

namespace XeroApp.Models.BusinessModels
{
    public class BatchPaymentReportModel
    {

        public string MyCompanyName { get; set; }

        public string MyCompanyAddress1 { get; set; }

        public string MyCompanyAddress2 { get; set; }

        public string MyCompanyAddress3 { get; set; }

        public string MyCompanyCityPostal { get; set; }

        public string BankAccountNumber { get; set; }

        public string BankAccount { get; set; }
        public Payments Payments { get; set; }

    }
}
