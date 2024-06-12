using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class TransactionTrack : BaseEntity
    {
        public string EmailAddress { get; set; }
        public string TransactionType { get; set; }
        public string TransactionValues { get; set; }
    }
}
