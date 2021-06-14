using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor.DB.Models
{
    public class Vote
    {
        public string IPAddress { get; set; }
        public int FlameId { get; set; }
        public int Adjustment { get; set; }
    }
}
