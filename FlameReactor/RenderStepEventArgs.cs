using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor
{
    public class RenderStepEventArgs : EventArgs
    {
        public string StepData { get; set; }
        public string FramePath { get; set; }
        public int PercentDone { get; set; }
        public RenderStepEventArgs(string stepData, string framePath, int percentDone)
        {
            StepData = stepData;
            FramePath = framePath;
            PercentDone = percentDone;
        }
    }
}
