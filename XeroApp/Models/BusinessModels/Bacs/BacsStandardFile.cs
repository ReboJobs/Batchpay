using System;
using System.Collections.Generic;
using System.Text;
using FileHelpers;

namespace XeroApp.Models.BusinessModels.Bacs
{
    [FixedLengthRecord]
    public class BacsStandardFile
    {
        [FieldFixedLength(10)]
        public VolRecord VolRecord { get; set; }

        public BacsStandardFile()
        {
            VolRecord = new VolRecord();
        }
    }
}
