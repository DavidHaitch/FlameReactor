using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor.DB.Models
{
    public class Breeding
    {
        public int ID { get; set; }
        public List<Flame> Parents { get; } = new List<Flame>();
        public int ChildID { get; set; }
        public Flame Child { get; set; }
    }
}
