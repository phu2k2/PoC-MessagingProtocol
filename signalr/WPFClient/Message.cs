using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFClient
{
    public class Message
    {
        public string? User { get; set; }
        public string? Room { get; set; }

        public string? MessageText { get; set; }

        public bool Status { get; set; }
    }
}
