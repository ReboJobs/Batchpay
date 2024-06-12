using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FileHelpers;


namespace XeroApp.Models.BusinessModels.Bacs
{
    [FixedLengthRecord()]
    public class HDR1Record
    {
        [FieldFixedLength(3)]
        public string LabelIdentifier1 { get; set; } = "HDR";

        [FieldFixedLength(1)]
        public string LabelIdentifier2 { get; set; } = "1";

        [FieldFixedLength(1)]
        public string FileIdentifierPos5 { get; set; } = "A";

        [FieldFixedLength(6)]
        public string FileIdentifierPos6To11 { get; set; } //Same as VOL Record

        [FieldFixedLength(1)]
        public string FileIdentifierPos12 { get; set; } = "S";

        [FieldFixedLength(2)]
        public string FileIdentifierPos13To14 { get; set; }

        [FieldFixedLength(1)]
        public string FileIdentifierPos15 { get; set; } = "1";

        [FieldFixedLength(6)]
        public string FileIdentifierPos16To21{ get; set; } //Same as VOL Record

        [FieldFixedLength(6)]
        public string SetIdentification { get; set; } //Same as VOL Record

        [FieldFixedLength(4)]
        public string FileSectionNumber { get; set; } = "0001";

        [FieldFixedLength(4)]
        public string FileSequenceNumber { get; set; } = "0001";

        [FieldFixedLength(4)]
        public string GenerationNumber { get; set; }

        [FieldFixedLength(2)]
        public string GenerationVersionNumber { get; set; }

        [FieldFixedLength(6)]
        [FieldAlign(AlignMode.Right, ' ')]
        public string CreationDate { get; set; } //UHL1 record

        [FieldFixedLength(6)]
        [FieldAlign(AlignMode.Right, ' ')]
        public string ExpirationDate { get; set; } //UHL1 record

        [FieldFixedLength(1)]
        [FieldAlign(AlignMode.Center, '0')]
        public string AccessibilityIndicator { get; set; }

        [FieldFixedLength(6)]
        [FieldAlign(AlignMode.Center, '0')]
        public string BlockCount { get; set; }

        [FieldFixedLength(13)]
        public string SystemCode { get; set; }

        [FieldFixedLength(7)]
        public string ReservedField { get; set; }

        public HDR1Record()
        {
            //this.FileIdentifier = string.Concat(FileIdentifierPos5, FileIdentifierPos6To11, FileIdentifierPos12);

        }

        public void InitializedDependentFields(VolRecord volRecord)
        {
            this.FileIdentifierPos6To11 = volRecord.OwnerIdPos42To47;
            //this.FileIdentifierPos16To21 = volRecord.OwnerIdPos42To47;
            this.SetIdentification = volRecord.SerialNumber;
        }


        public string CreationDateCondition(UHL1Record UHL1Record)
        {

            DateTime convertedTransactiondate;

            if (DateTime.TryParseExact(UHL1Record.BacsProcessingDay, "MM/dd/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out convertedTransactiondate))
            {

                return String.Concat(convertedTransactiondate.ToString("yy"), convertedTransactiondate.ToString("000"));
            }
            return string.Empty;
        }

        public string ExpirationDateCondition(UHL1Record UHL1Record)
        {
            DateTime convertedTransactiondate;

            if (DateTime.TryParseExact(UHL1Record.BacsProcessingDay, "MM/dd/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out convertedTransactiondate))
            {

                return String.Concat(convertedTransactiondate.ToString("yy"), convertedTransactiondate.ToString("000"));
            }
            return string.Empty;
        }

    }
}
