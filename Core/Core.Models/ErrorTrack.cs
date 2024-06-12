using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ErrorTrack : BaseEntity
    {
        public string EmailAddress { get; set; }

        public string TransactionValues { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorMessageDetail { get; set; }
    }
}
