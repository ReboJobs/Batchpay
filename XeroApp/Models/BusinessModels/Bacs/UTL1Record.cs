using System;
using System.Collections.Generic;
using System.Text;
using FileHelpers;

namespace XeroApp.Models.BusinessModels.Bacs
{
    [FixedLengthRecord()]
    public class UTL1Record
    {


        [FieldFixedLength(3)]
        public string LabelIdentifier { get; set; } = "UTL";

        [FieldFixedLength(1)]
        public string LabelNumber { get; set; } = "1";


        [FieldFixedLength(13)]
        [FieldAlign(AlignMode.Right, '0')]
        public string MonetaryTotalOfDebitRecords { get; set; }

        [FieldFixedLength(13)]
        [FieldAlign(AlignMode.Right, '0')]
        public string MonetaryTotalOfCreditRecords { get; set; }


        [FieldFixedLength(7)]
        [FieldAlign(AlignMode.Right, '0')]
        public string CountOfDebitRecords { get; set; } = "0000001";


        [FieldFixedLength(7)]
        [FieldAlign(AlignMode.Right, '0')]
        public string CountOfCreditRecords { get; set; } = "0000018";


        [FieldFixedLength(10)]
        public string ReservedForFutureStandardisation { get; set; }

        [FieldFixedLength(26)]
        public string ForUseByUserORBureau { get; set; }

    }
}
