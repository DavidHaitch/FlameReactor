using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor.DB.Models
{
    public class InteractionEvent
    {
        public DateTimeOffset Timestamp { get; set; }
        public string IPAddress { get; set; }
        public string InteractionType { get; set; }
        public string Details { get; set; }
    }
}
