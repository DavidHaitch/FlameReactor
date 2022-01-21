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
            GenomeTries = 100;
            LoopFrames = 450;
            Quality = 2500;
            TS = 20;
            MutationChance = 0.66;
            AlternateSetpoint = 0.45;
            UnionSetpoint = 0.9;
            Supersample = 2;
            MaxTransforms = 150;
            MaxScale = 20000;
            MaxDisplacement = 100;
            ResolutionMultiplier = 1;
            RenderResolutionMultiplier = 1.5;
            MotionDensity = 0.15;
            SelectionInstability = 0.1;
            AnimationDensity = 0.0;
            MaxBlur = 1.0;
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
        public double SelectionInstability { get; set; }
        public double AnimationDensity { get; set; }
        public double MaxBlur { get; set; }
    }
}
