using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor
{
    public class FlameConfig
    {
        public FlameConfig()
        {
            GenomeTries = 15;
            LoopFrames = 600;
            Quality = 1000;
            TS = 20;
            MutationChance = 0.25;
            AlternateSetpoint = 0.5;
            UnionSetpoint = 0.75;
            Supersample = 2;
            MaxTransforms = 20;
            MaxScale = 2000;
            MaxDisplacement = 5;
            ResolutionMultiplier = 1;
            RenderResolutionMultiplier = 1;
            MotionDensity = 0.15;
            PromiscuityDecay = 2;
        }

        public int GenomeTries { get; set; }
        public int MaxScale { get; set; }
        public int Supersample { get; set; }
        public double MutationChance { get; set; }
        public double MaxDisplacement { get; set; }
        public int LoopFrames { get; set; }
        public int Quality { get; set; }
        public int TS { get; set; }
        public double AlternateSetpoint { get; set; }
        public double UnionSetpoint { get; set; }
        public int MaxTransforms { get; set; }
        public double ResolutionMultiplier { get; set; }
        public double RenderResolutionMultiplier { get; set; }
        public double MotionDensity { get; set; }
        public double PromiscuityDecay { get; internal set; }
    }
}
