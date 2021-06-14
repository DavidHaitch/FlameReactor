using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor
{
    public class RenderCompleteEventArgs : EventArgs
    {
        public string VideoPath { get; set; }
        public RenderCompleteEventArgs(string videoPath)
        {
            VideoPath = videoPath;
        }
    }
}
