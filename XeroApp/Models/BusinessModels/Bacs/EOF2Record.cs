using System;
using System.Collections.Generic;
using System.Text;
using FileHelpers;


namespace XeroApp.Models.BusinessModels.Bacs
{
    [FixedLengthRecord()]
    public class EOF2Record
    {
        [FieldFixedLength(3)]
        public string LabelIdentifier { get; set; } = "EOF";

        [FieldFixedLength(1)]
        public string LabelNumber { get; set; } = "2";

        #region sameAsHDR2 POS 5 - 80

        [FieldFixedLength(1)]
        public string RecordFormat { get; set; } = "F";

        [FieldFixedLength(5)]
        public string BlockLength { get; set; } = "02000";

        [FieldFixedLength(5)]
        public string RecordLength { get; set; }

        [FieldFixedLength(35)]
        public string ReservedForOperatingSystems { get; set; }

        [FieldFixedLength(2)]
        [FieldAlign(AlignMode.Center, '0')]
        public string BufferOffset { get; set; }

        [FieldFixedLength(28)]
        public string ReservedField { get; set; }
        #endregion

        public void InitializedDependentFields(HDR2Record hDR2Record) {

            this.RecordLength = hDR2Record.RecordLength;
            this.ReservedForOperatingSystems = hDR2Record.ReservedForOperatingSystems;
            this.BufferOffset = hDR2Record.BufferOffset;
            this.ReservedField = hDR2Record.ReservedField;
        }

    }
}
