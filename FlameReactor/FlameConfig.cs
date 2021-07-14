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
            GenomeTries = 10;
            LoopFrames = 450;
            Quality = 1000;
            TS = 20;
            MutationChance = 0.33;
            AlternateSetpoint = 0.45;
            UnionSetpoint = 0.9;
            Supersample = 2;
            MaxTransforms = 15;
            MaxScale = 1000;
            MaxDisplacement = 2;
            ResolutionMultiplier = 1;
            RenderResolutionMultiplier = 1.5;
            MotionDensity = 0.15;
            SelectionInstability = 4;
            AnimationDensity = 0.1;
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
        public int SelectionInstability { get; internal set; }
        public double AnimationDensity { get; set; }
    }
}
