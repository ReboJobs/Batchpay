using System;
using System.Collections.Generic;
using System.Text;
using FileHelpers;

namespace XeroApp.Models.BusinessModels.Bacs
{
    [FixedLengthRecord()]
    public class VolRecord
    {
        [FieldFixedLength(3)]
        public string LabelIdentifier1 { get; set; } = "VOL";

        [FieldFixedLength(1)]
        public string LabelIdentifier2 { get; set; } = "1";

        [FieldFixedLength(6)]
        public string SerialNumber { get; set; } = "025590";

        [FieldFixedLength(1)]
        public string AccessibilityIndicator { get; set; }

        [FieldFixedLength(20)]
        public string ReservedField { get; set; }

        [FieldFixedLength(6)]
        public string ReservedField2 { get; set; } = "HSBC";

        [FieldFixedLength(4)]
        public string OwnerIdPos38To41 { get; set; }

        [FieldFixedLength(6)]
        public string OwnerIdPos42To47 { get; set; }

        [FieldFixedLength(4)]
        public string OwnerIdPos48To51 { get; set; }

        [FieldFixedLength(28)]
        public string ReservedField3 { get; set; }

        [FieldFixedLength(1)]
        public string LabelStandardLevel { get; set; } = "1";



        public void InitializedDependentFields()
        {
            //No Dependent
            
        }

        public string OwnerIdPos42To47Condition(string Value) {

            if (string.IsNullOrEmpty(this.ReservedField2))
                return Value;
            else
                return string.Empty;
        }
    }
}
