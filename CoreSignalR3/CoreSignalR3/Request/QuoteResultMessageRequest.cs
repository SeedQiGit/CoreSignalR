using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreSignalR3.Request
{
    public class QuoteResultMessageRequest
    {
        public long Buid { get; set; }

        public long EmployeeId { get; set; }


        public int Source { get; set; }

        public string guid { get; set; }
    }
    public class QuoteResultMessage
    {
        public long Buid { get; set; }

        public int Source { get; set; }

        public string Guid { get; set; }

    }
}
