using FlameReactor.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor.FlameActions
{
    class FixColorPalettesAction : FlameAction
    {
        public FixColorPalettesAction(Action<RenderStepEventArgs> stepEvent) : base(stepEvent) { }

        public override async Task<Flame[]> Act(FlameConfig flameConfig, params Flame[] flames)
        {
            for (var i = 0; i < flames.Length; i++)
            {
                var f = await FixColorPalette(flameConfig, flames[i]);
                if (f != null) flames[i] = f;
            }

            return flames;
        }
        private async Task<Flame> FixColorPalette(FlameConfig flameConfig, Flame flame)
        {
            StepEvent(new RenderStepEventArgs("Optimizing colors", "", 10));

            var mutateProcess = await Util.RunProcess(EnvironmentPaths.EmberGenomePath,
                new[] {"--debug", "--tries=" + flameConfig.GenomeTries, "--mutate=" + flame.GenomePath, "--method=color_palette" },
                flame.GenomePath);

            mutateProcess.WaitForExit();
            var exitCode = mutateProcess.ExitCode;
            if (exitCode == 0)
            {
                flame.Update();
                return flame;
            }

            return null;
        }
    }
}
