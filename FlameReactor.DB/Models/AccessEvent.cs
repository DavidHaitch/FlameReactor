using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor.DB.Models
{
    public class AccessEvent
    {
        public DateTimeOffset Timestamp { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public string Referrer { get; set; }
    }
}
