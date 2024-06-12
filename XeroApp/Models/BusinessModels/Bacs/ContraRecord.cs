using System;
using System.Collections.Generic;
using System.Text;
using FileHelpers;

namespace XeroApp.Models.BusinessModels.Bacs
{
    [FixedLengthRecord()]
    public class ContraRecord
    {

        [FieldFixedLength(6)]
        public string OriginatingSortCodeNumber { get; set; }

        [FieldFixedLength(8)]
        public string OriginatingAccountNumber { get; set; }

        [FieldFixedLength(1)]
        [FieldAlign(AlignMode.Left, '0')]
        public string TypeOfUsersAccount { get; set; }

        [FieldFixedLength(2)]
        public string TransactionCode { get; set; } = "17";

        [FieldFixedLength(6)]
        public string OriginatingSortCodeNumber2 { get; set; }

        [FieldFixedLength(8)]
        public string OriginatingAccountNumber2 { get; set; }

        [FieldFixedLength(4)]
        public string FreeFormat { get; set; }

        [FieldFixedLength(11)]
        [FieldAlign(AlignMode.Right, '0')]
        public string Amount { get; set; } // Dependent on Standard Record Total

        [FieldFixedLength(18)]
        //[FieldAlign(AlignMode.Left, ' ')]
        public string NarrativeOfUsersChoice { get; set; }

        [FieldFixedLength(18)]
        [FieldAlign(AlignMode.Left, ' ')]
        public string ContraIdentification { get; set; } = "CONTRA";


        [FieldFixedLength(18)]
        [FieldAlign(AlignMode.Left, ' ')]
        public string AbbreviatedAccountNameOfUserNominatedAccount { get; set; }

        //[FieldFixedLength(6)]
        //public string BacsProcessingDayOfData { get; set; }


        public ContraRecord()
        {
          
        }

        public void InitializedDependentFields(string OriginatingSortCodeNumber,
                            string OriginatingAccountNumber,
                            List<StandardRecords> standardRecordslist,
                            string NarrativeOfUsersChoice,
                            string AbbreviatedAccountNameOfUserNominatedAccount) 
        {

            this.OriginatingSortCodeNumber = OriginatingSortCodeNumber;
            this.OriginatingAccountNumber = OriginatingAccountNumber;
            this.OriginatingSortCodeNumber2 = OriginatingSortCodeNumber;
            this.OriginatingAccountNumber2 = OriginatingAccountNumber;
            this.Amount = standardRecordslist.Sum(sr => sr.Amount).ToString();
            this.NarrativeOfUsersChoice = NarrativeOfUsersChoice;
            this.AbbreviatedAccountNameOfUserNominatedAccount = AbbreviatedAccountNameOfUserNominatedAccount;


        }
    }
}
