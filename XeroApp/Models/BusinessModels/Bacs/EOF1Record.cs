using System;
using System.Collections.Generic;
using System.Text;
using FileHelpers;


namespace XeroApp.Models.BusinessModels.Bacs
{
    [FixedLengthRecord()]
    public class EOF1Record
    {
        [FieldFixedLength(3)]
        public string LabelIdentifier { get; set; } = "EOF";

        [FieldFixedLength(1)]
        public string LabelNumber { get; set; } = "1";

        #region SameAsHDR1 POS 5 - 54

        [FieldFixedLength(1)]
        public string FileIdentifierPos5 { get; set; }

        [FieldFixedLength(6)]
        public string FileIdentifierPos6To11 { get; set; }

        [FieldFixedLength(1)]
        public string FileIdentifierPos12 { get; set; }

        [FieldFixedLength(2)]
        public string FileIdentifierPos13To14 { get; set; }

        [FieldFixedLength(1)]
        public string FileIdentifierPos15 { get; set; }

        [FieldFixedLength(6)]
        public string FileIdentifierPos16To21 { get; set; }

        [FieldFixedLength(6)]
        public string SetIdentification { get; set; }

        [FieldFixedLength(4)]
        public string FileSectionNumber { get; set; }

        [FieldFixedLength(4)]
        public string FileSequenceNumber { get; set; }

        [FieldFixedLength(4)]
        public string GenerationNumber { get; set; }

        [FieldFixedLength(2)]
        public string GenerationVersionNumber { get; set; }

        [FieldFixedLength(6)]
        [FieldAlign(AlignMode.Right, ' ')]
        public string CreationDate { get; set; }

        [FieldFixedLength(6)]
        [FieldAlign(AlignMode.Right, ' ')]
        public string ExpirationDate { get; set; }

        [FieldFixedLength(1)]
        [FieldAlign(AlignMode.Center, '0')]
        public string AccessibilityIndicator { get; set; }
        #endregion

        [FieldFixedLength(6)]
        [FieldAlign(AlignMode.Center, '0')]
        public string BlockCount { get; set; }

        #region SameAsHDR1 POS 61 - 80

        [FieldFixedLength(13)]
        public string SystemCode { get; set; }

        [FieldFixedLength(7)]
        public string ReservedField { get; set; }

        #endregion

        public EOF1Record()
        {
          
        }

        public void InitializedDependentFields(HDR1Record hDR1Record) {

            this.FileIdentifierPos5 = hDR1Record.FileIdentifierPos5;
            this.FileIdentifierPos6To11 = hDR1Record.FileIdentifierPos6To11;
            this.FileIdentifierPos12 = hDR1Record.FileIdentifierPos12;
            this.FileIdentifierPos13To14 = hDR1Record.FileIdentifierPos13To14;
            this.FileIdentifierPos15 = hDR1Record.FileIdentifierPos15;
            this.FileIdentifierPos16To21 = hDR1Record.FileIdentifierPos16To21;
            this.SetIdentification = hDR1Record.SetIdentification;
            this.FileSectionNumber = hDR1Record.FileSectionNumber;
            this.FileSequenceNumber = hDR1Record.FileSequenceNumber;
            this.GenerationNumber = hDR1Record.GenerationNumber;
            this.GenerationVersionNumber = hDR1Record.GenerationVersionNumber;
            this.CreationDate = hDR1Record.CreationDate;
            this.ExpirationDate = hDR1Record.ExpirationDate;
            this.AccessibilityIndicator = hDR1Record.AccessibilityIndicator;

            this.SystemCode = hDR1Record.SystemCode;
            this.ReservedField = hDR1Record.ReservedField;

        }
    }
}
