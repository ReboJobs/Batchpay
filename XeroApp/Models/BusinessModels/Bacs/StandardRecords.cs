using System;
using System.Collections.Generic;
using System.Text;
using FileHelpers;


namespace XeroApp.Models.BusinessModels.Bacs
{
    [FixedLengthRecord()]
    public class StandardRecords
    {
        [FieldFixedLength(6)]
        [FieldAlign(AlignMode.Left, '1')]
        public string DestinationSortCodeNumber { get; set; }

        [FieldFixedLength(8)]
        [FieldAlign(AlignMode.Left, '1')]
        public string DestinationAccountNumber { get; set; }

        [FieldFixedLength(1)]
        [FieldAlign(AlignMode.Left, '0')]
        public string DestinationTypeAccount { get; set; }

        [FieldFixedLength(2)]
        public string TransactionCode { get; set; } = "99";

        [FieldFixedLength(6)]
        public string OriginatingSortCodeNumber { get; set; }

        [FieldFixedLength(8)]
        public string OriginatingAccountNumber { get; set; }

        [FieldFixedLength(4)]
        public string FreeFormat { get; set; }

        [FieldFixedLength(11)]
        [FieldAlign(AlignMode.Right, '0')]
        public int Amount { get; set; }

        [FieldFixedLength(18)]
        [FieldAlign(AlignMode.Left, ' ')]
        public string Username { get; set; }

        [FieldFixedLength(18)]
        [FieldAlign(AlignMode.Left, ' ')]
        public string UserReference { get; set; }

        [FieldFixedLength(18)]
        [FieldAlign(AlignMode.Left, ' ')]
        public string DestinationAccountName { get; set; }

        //[FieldFixedLength(6)]
        //[FieldAlign(AlignMode.Left, ' ')]
        //public string BacsProcessingDayOfData { get; set; }


    }
}
