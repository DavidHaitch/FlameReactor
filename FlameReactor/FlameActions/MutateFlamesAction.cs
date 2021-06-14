using FlameReactor.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FlameReactor.FlameActions
{
    class MutateFlamesAction : FlameAction
    {
        public MutateFlamesAction(Action<RenderStepEventArgs> stepEvent) : base(stepEvent) { }
        public override async Task<Flame[]> Act(FlameConfig flameConfig, params Flame[] flames)
        {
            for (var i = 0; i < flames.Length; i++)
            {
                var f = await MutateFlame(flameConfig, flames[i]);
                if (f != null) flames[i] = f;
            }

            return flames;
        }

        private async Task<Flame> MutateFlame(FlameConfig flameConfig, Flame flame)
        {
            var r = Util.Rand.NextDouble();
            if (r > flameConfig.MutationChance) return flame;
            var methods = new List<string>()
            {
                "all_vars",
                "one_xform",
                "one_xform",
                "addMotion",
                "delete_xform",
                "add_symmetry",
                //"post_xforms",
                "all_coefs"
            };

            var friendlyMethods = new List<string>()
            {
                "all variations",
                "one gene",
                "one gene",
                "by adding motion",
                "by removing a gene",
                "by adding symmetry",
                //"post-affines",
                "all coefficients"
            };

            var methodIdx = Util.Rand.Next(0, methods.Count);
            var method = methods[methodIdx];
            
            StepEvent(new RenderStepEventArgs("Mutating " + friendlyMethods[methodIdx], "", 10));
            if (method == "addMotion")
            {
                flame = InsertMotion(flame, flameConfig);
            }
            else
            {
                var mutateProcess = await Util.RunProcess(EnvironmentPaths.EmberGenomePath,
                    new[] { "--debug", "--tries=" + flameConfig.GenomeTries, "--mutate=" + flame.GenomePath, "--method=" + method, "--maxxforms=" + flameConfig.MaxTransforms, "--noedits" },
                    flame.GenomePath);

                mutateProcess.WaitForExit(5 * 60 * 1000);
                if (!mutateProcess.HasExited)
                {
                    mutateProcess.Kill();
                    return flame;
                }
            }

            flame.Update();
            return flame;
        }

        private Flame InsertMotion(Flame flame, FlameConfig flameConfig)
        {
            var doc = XDocument.Load(flame.GenomePath);

            foreach (var n in doc.Descendants("flame"))
            {
                foreach(var xform in doc.Descendants("xform"))
                {
                    if (Util.Rand.NextDouble() > flameConfig.MotionDensity) continue;
                    var name = xform.Attributes().Where(a => double.TryParse(a.Value, out var irrelevant) && a.Name != "symmetry" && a.Name != "animate" && a.Name != "name").OrderBy(a => Util.Rand.Next()).Last().Name;
                    xform.Add(CreateMotionElement(xform, name.LocalName));
                    xform.SetAttributeValue(name, null);
                }

                foreach (var xform in doc.Descendants("finalxform"))
                {
                    if (Util.Rand.NextDouble() > flameConfig.MotionDensity) continue;
                    var name = xform.Attributes().Where(a => double.TryParse(a.Value, out var irrelevant) && a.Name != "symmetry" && a.Name != "animate" && a.Name != "name").OrderBy(a => Util.Rand.Next()).Last().Name;
                    xform.Add(CreateMotionElement(xform, name.LocalName));
                    xform.SetAttributeValue(name, null);
                }
            }

            using (var writer = new XmlTextWriter(flame.GenomePath, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                doc.Save(writer);
            }

            return flame;
        }

        private XElement CreateMotionElement(XElement parent, string name)
        {
            var motionFunctions = new List<string>() { "sin", "sin", "triangle", "hill", "hill" };
            var motionFrequencies = new List<string>() { "1", "2", "4", "8" };

            var motionAttrName = name;
            var motionAttr = new XAttribute(motionAttrName, (Util.Rand.NextDouble() + 0.5).ToString().Substring(0, 3));
            var motionFunction = motionFunctions[Util.Rand.Next(0, motionFunctions.Count)];
            var motionFrequency = motionFrequencies[Util.Rand.Next(0, motionFrequencies.Count)];
            var attributes = new List<XAttribute>();
            attributes.Add(motionAttr);
            Console.WriteLine("Adding a motion element for " + motionAttrName);
            attributes.Add(new XAttribute("motion_offset", "1"));
            attributes.Add(new XAttribute("motion_frequency", motionFrequency));
            attributes.Add(new XAttribute("motion_function", motionFunction));
            var element = new XElement("motion", attributes.ToArray());
            element.SetAttributeValue("symmetry", "0");
            return element;
        }
    }
}
