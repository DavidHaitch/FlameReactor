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
            MinScale = 250;
            MaxScale = 20000;
            CenterMagnetism = 0.25;
            MaxDisplacement = 100;
            ResolutionMultiplier = 1;
            RenderResolutionMultiplier = 1.5;
            MotionDensity = 0.15;
            SelectionInstability = 1;
            AnimationDensity = 0.0;
            OrbitDensity = 0.0;
            MaxBlur = 1.0;
            CvSettings = new CVSettings();
            MutationWeights = new Dictionary<string, int>()
            {
                {"all_vars", 20 },
                {"color_points", 20},
                {"one_xform", 40},
                {"delete_xform", 20},
                {"add_symmetry", 30},
                {"post_xforms", 30},
                {"all_coefs", 40},
                {"addMotion", 2},
                {"addOrbit", 20},
                {"addChaos", 2},
                {"randomizeAnimation", 20 }
            };
        }

        public CVSettings CvSettings { get; set; }
        public Dictionary<string, int> MutationWeights { get; set; }
        public int GenomeTries { get; set; }
        public int MinScale { get; set; }
        public int MaxScale { get; set; }
        public int Supersample { get; set; }
        public double MutationChance { get; set; }
        public double MaxDisplacement { get; set; }
        public double CenterMagnetism { get; set; }
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
        public double OrbitDensity { get; set; }
    }

    public class CVSettings
    {
        public CVSettings()
        {
            MinRating = 95;
            BlackFractionMin = 0.001;
            BlackFractionMax = 0.1;
            ContoursMin = 600;
            ContoursMax = 6000;
            SharpnessMin = 800;
            SharpnessMax = 100000;
            BlackFractionWeight = 25;
            ContourWeight = 40;
            SharpnessWeight = 35;
        }

        public double MinRating { get; set; }
        public double BlackFractionMin { get; set; }
        public double BlackFractionMax { get; set; }
        public double ContoursMin { get; set; }
        public double ContoursMax { get; set; }
        public double SharpnessMin { get; set; }
        public double SharpnessMax { get; set; }
        public double BlackFractionWeight { get; set; }
        public double ContourWeight { get; set; }
        public double SharpnessWeight { get; set; }
    }
}
