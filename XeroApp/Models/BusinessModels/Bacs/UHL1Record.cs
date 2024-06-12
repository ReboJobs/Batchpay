using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FileHelpers;

namespace XeroApp.Models.BusinessModels.Bacs
{
    [FixedLengthRecord()]
    public class UHL1Record
    {

        [FieldFixedLength(3)]
        public string LabelIdentifier { get; set; } = "UHL";

        [FieldFixedLength(1)]
        public string LabelNumber { get; set; } = "1";

        [FieldFixedLength(6)]
        [FieldAlign(AlignMode.Right, ' ')]
        public string BacsProcessingDay { get; set; } //UHL1 record

        [FieldFixedLength(10)]
        [FieldAlign(AlignMode.Left, ' ')]
        public string IdentifyingNumberOfReceivingParty { get; set; } = "999999";

        [FieldFixedLength(2)]
        public string CurrencyCode { get; set; } = "00";

        [FieldFixedLength(6)]
        [FieldAlign(AlignMode.Center, ' ')]
        public string CountryCode { get; set; }

        [FieldFixedLength(9)]
        [FieldConverter(typeof(bSpaceConverter))]
        public string WorkCode { get; set; } = "1bDAILYbb";

        [FieldFixedLength(3)]
        public string FileNumber { get; set; } = "001";

        [FieldFixedLength(7)]
        public string ReservedField { get; set; }

        [FieldFixedLength(3)]
        public string AuditPrintIdentifierPos48To50 { get; set; }

        [FieldFixedLength(4)]
        public string AuditPrintIdentifierPos51To54 { get; set; }

        [FieldFixedLength(26)]
        public string ForUseByUserBureau { get; set; }


        public UHL1Record()
        {

        }

        public string BacsProcessingDayCondition(string transactiondate) {
            DateTime convertedTransactiondate;

            if (DateTime.TryParseExact(transactiondate, "MM/dd/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out convertedTransactiondate))
            {  
                return String.Concat(convertedTransactiondate.ToString("yy"), convertedTransactiondate.DayOfYear.ToString());
            }
            return string.Empty;
        }
    }
}
