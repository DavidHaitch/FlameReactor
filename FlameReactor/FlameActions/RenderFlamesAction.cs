using FlameReactor.DB.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor.FlameActions
{
    class RenderFlamesAction : FlameAction
    {
        public RenderFlamesAction(Action<RenderStepEventArgs> stepEvent) : base(stepEvent) { }
        public override async Task<Flame[]> Act(FlameConfig flameConfig, params Flame[] flames)
        {
            for(var i = 0; i < flames.Length; i++)
            { 
                flames[i] = await RenderFlame(flameConfig, flames[i]);
            }

            return flames;
        }

        private async Task<Flame> RenderFlame(FlameConfig flameConfig, Flame flame)
        {
            StepEvent(new RenderStepEventArgs("Rendering", string.Empty, 15));
            var render = await Util.RunProcess(EnvironmentPaths.EmberRenderPath, new[] { "--quality=" + 2000, "--demax=4", "--sp", "--supersample=2", "--opencl", "--prefix=" + flame.Name + ".", "--in=" + flame.GenomePath,
                            "--hs=" + flameConfig.RenderResolutionMultiplier,
                "--ws=" + flameConfig.RenderResolutionMultiplier,
                "--ss=" + flameConfig.RenderResolutionMultiplier});
            render.WaitForExit(5 * 60 * 1000);
            if (!render.HasExited || render.ExitCode != 0) throw new Exception("Render failed");
            flame.Update();
            MakeThumbnail(flame.ImagePath);
            StepEvent(new RenderStepEventArgs("Render Complete", flame.ImagePathWeb, 15));
            return flame;
        }

        private void MakeThumbnail(string file)
        {
            using (Image image = Image.Load(file))
            {
                image.Mutate(x => x.Resize(image.Width / 4, image.Height / 4));

                var thumb = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + "_thumb.png");
                image.Save(thumb);
            }
        }
    }
}
