using System;
using System.Collections.Generic;
using System.Text;
using FileHelpers;

namespace XeroApp.Models.BusinessModels.Bacs
{
    [FixedLengthRecord()]
    public class HDR2Record
    {

        [FieldFixedLength(3)]
        public string LabelIdentifier { get; set; } = "HDR";

        [FieldFixedLength(1)]
        public string LabelNumber { get; set; } = "2";

        [FieldFixedLength(1)]
        public string RecordFormat { get; set; } = "F";

        [FieldFixedLength(5)]
        public string BlockLength { get; set; } = "02000";

        [FieldFixedLength(5)]
        public string RecordLength { get; set; } = "00100";

        [FieldFixedLength(35)]
        public string ReservedForOperatingSystems { get; set; }

        [FieldFixedLength(2)]
        [FieldAlign(AlignMode.Center, '0')]
        public string BufferOffset { get; set; }

        [FieldFixedLength(28)]
        public string ReservedField { get; set; }
    }

}
