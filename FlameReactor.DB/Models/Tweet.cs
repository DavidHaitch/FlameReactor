using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor.DB.Models
{
    public class TweetRecord
    {
        public ulong ID { get; set; }
        public int Faves { get; set; }
        public int Retweets { get; set; }
        public Flame Owner { get; set; }
    }
}
