using FlameReactor.DB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor.FlameActions
{
    class CrossFlamesAction : FlameAction
    {
        public CrossFlamesAction(Action<RenderStepEventArgs> stepEvent) : base(stepEvent) { }

        public override async Task<Flame[]> Act(FlameConfig flameConfig, params Flame[] flames)
        {
            if (flames.Length % 2 != 0)
            {
                throw new InvalidOperationException("CrossFlamesAction requires an even number of breeding partners.");
            }

            var childFlames = new List<Flame>();
            for(var i = 0; i < flames.Length; i+=2)
            {
                var f = await CrossFlames(flameConfig, flames[i], flames[i + 1]);
                if (f != null) childFlames.Add(f);
            }

            return childFlames.ToArray();
        }

        private async Task<Flame> CrossFlames(FlameConfig flameConfig, Flame flame1, Flame flame2)
        {
            flame1.Promiscuity++;
            flame2.Promiscuity++;

            var childName = Path.Combine(EnvironmentPaths.FlamePoolPath, DateTime.Now.Ticks.ToString() + ".flame");
            var r = Util.Rand.NextDouble();
            var method = "interpolate";
            var friendlyMethod = " via interpolation";
            if (r >= flameConfig.AlternateSetpoint && r <= flameConfig.UnionSetpoint)
            {
                method = "alternate";
                friendlyMethod = " via alternation";
            }
            else if (r > flameConfig.UnionSetpoint)
            {
                method = "union";
                friendlyMethod = " via union";
            }
            StepEvent(new RenderStepEventArgs("Breeding" + friendlyMethod, method, 5));
            var crossProcess = await Util.RunProcess(EnvironmentPaths.EmberGenomePath,
                new[] { "--debug", "--opencl", "--sp", "--tries=" + flameConfig.GenomeTries, "--cross0=" + flame1.GenomePath, "--cross1=" + flame2.GenomePath, method, "--maxxforms=" + flameConfig.MaxTransforms, "--noedits" },
                //new[] { "--opencl", "--quality=" + 500, "--tries=" + flameConfig.GenomeTries, "--cross0=" + flame1.GenomePath, "--cross1=" + flame2.GenomePath, "--method=" + method },
                childName);

            crossProcess.WaitForExit();
            var exitCode = crossProcess.ExitCode;
            if (exitCode == 0)
            {
                var names = File.ReadAllLines("./mysteryNames.txt").ToList();
                var child = new Flame(childName);
                var name = names[Util.Rand.Next(names.Count())];
                child.DisplayName = name.First().ToString().ToUpper() + name.Substring(1).ToLower();
                child.Generation = flame1.Generation >= flame2.Generation ? flame1.Generation + 1 : flame2.Generation + 1;
                child.Genome = File.ReadAllText(child.GenomePath);
                child.Birth = new Breeding();
                child.Birth.Parents.Add(flame1);
                child.Birth.Parents.Add(flame2);
                return child;
            }

            return null;
        }
    }
}
