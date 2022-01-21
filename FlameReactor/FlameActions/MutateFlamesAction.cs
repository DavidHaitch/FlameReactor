using FlameReactor.DB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FlameReactor.FlameActions
{


    class MutateFlamesAction : FlameAction
    {
        private class MutationMethod
        {
            public string MethodName;
            public string FriendlyMethodName;
            public double Weight;
        }

        private List<string> allVars = new List<string>();
        public MutateFlamesAction(Action<RenderStepEventArgs> stepEvent) : base(stepEvent)
        {
            allVars = File.ReadAllLines("./allvars.txt").ToList();
        }

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
            var methods = new List<MutationMethod>()
            {
                new MutationMethod(){MethodName =  "all_vars", Weight = 20, FriendlyMethodName = "all variations"},
                new MutationMethod(){MethodName =  "color_points", Weight = 20, FriendlyMethodName = "color points"},
                new MutationMethod(){MethodName =  "one_xform", Weight = 40, FriendlyMethodName = "one gene"},
                new MutationMethod(){MethodName =  "delete_xform", Weight = 20, FriendlyMethodName = "by removing a gene"},
                new MutationMethod(){MethodName =  "add_symmetry", Weight = 30, FriendlyMethodName = "by adding symmetry"},
                new MutationMethod(){MethodName =  "post_xforms", Weight = 30, FriendlyMethodName = "post-affines"},
                new MutationMethod(){MethodName =  "all_coefs", Weight = 40, FriendlyMethodName = "all coefficients"},
                new MutationMethod(){MethodName =  "addMotion", Weight = 10, FriendlyMethodName = "by adding motion"},
                new MutationMethod(){MethodName =  "addChaos", Weight = 4, FriendlyMethodName = "by adding chaos"},
                new MutationMethod(){MethodName =  "randomizeAnimation", Weight = 20, FriendlyMethodName = "by randomizing animations"}
            };

            methods = methods.OrderBy(m => Util.Rand.NextDouble()).ToList();
            var probabilitySum = methods.Sum(m => m.Weight);
            var selection = Util.Rand.NextDouble() * probabilitySum;
            var selectedMethod = methods[0];
            foreach (var method in methods)
            {
                selection -= method.Weight;
                if (selection > 0)
                    continue;
                selectedMethod = method;
                break;
            }

            StepEvent(new RenderStepEventArgs("Mutating " + selectedMethod.FriendlyMethodName, "", 10));

            if (selectedMethod.MethodName == "addMotion")
            {
                flame = InsertMotion(flame, flameConfig);
                Thread.Sleep(2000);
            }
            else if (selectedMethod.MethodName == "addChaos")
            {
                flame = AddChaos(flame, flameConfig);
                Thread.Sleep(2000);
            }
            else if (selectedMethod.MethodName == "randomizeAnimation")
            {
                flame = RandomizeAnimations(flame, flameConfig);
                Thread.Sleep(2000);
            }
            else if (selectedMethod.MethodName == "color_points")
            {
                flame = RandomizeColorPoints(flame, flameConfig);
                Thread.Sleep(2000);
            }
            else
            {
                var mutateProcess = await Util.RunProcess(EnvironmentPaths.EmberGenomePath,
                    new[] { "--debug", "--opencl", "--speed=2", "--sp", "--tries=" + flameConfig.GenomeTries, "--mutate=" + flame.GenomePath, "--method=" + selectedMethod.MethodName, "--maxxforms=" + flameConfig.MaxTransforms, "--noedits" },
                    flame.GenomePath);

                mutateProcess.WaitForExit(5 * 60 * 1000);
                if (!mutateProcess.HasExited)
                {
                    mutateProcess.Kill();
                    return flame;
                }
            }

            flame.Update();
            flame.Genome = File.ReadAllText(flame.GenomePath);
            return flame;
        }
        private Flame RandomizeColorPoints(Flame flame, FlameConfig flameConfig)
        {
            var doc = XDocument.Load(flame.GenomePath);

            foreach (var n in doc.Descendants("flame"))
            {
                var xformCount = doc.Descendants("xform").Count();
                foreach (var xform in doc.Descendants("xform"))
                {
                    xform.SetAttributeValue("color", Util.Rand.NextDouble());
                }
            }

            using (var writer = new XmlTextWriter(flame.GenomePath, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                doc.Save(writer);
            }

            return flame;
        }

        private Flame AddChaos(Flame flame, FlameConfig flameConfig)
        {
            var doc = XDocument.Load(flame.GenomePath);

            foreach (var n in doc.Descendants("flame"))
            {
                var xformCount = doc.Descendants("xform").Count();
                foreach (var xform in doc.Descendants("xform"))
                {
                    xform.SetAttributeValue("chaos", GetChaosSequence(xformCount, 0.25));
                }
            }

            using (var writer = new XmlTextWriter(flame.GenomePath, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                doc.Save(writer);
            }

            return flame;
        }

        private Flame RandomizeAnimations(Flame flame, FlameConfig flameConfig)
        {
            var doc = XDocument.Load(flame.GenomePath);

            foreach (var n in doc.Descendants("flame"))
            {
                var xformCount = doc.Descendants("xform").Count();
                var xforms = doc.Descendants("xform");
                foreach (var xform in xforms)
                {
                    xform.SetAttributeValue("animate", 0);
                }

                for (var i = 0; i <= xformCount * flameConfig.AnimationDensity; i++)
                {
                    xforms.ElementAt(Util.Rand.Next(0, xformCount)).SetAttributeValue("animate", 1);
                }
            }

            using (var writer = new XmlTextWriter(flame.GenomePath, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                doc.Save(writer);
            }

            return flame;
        }

        private string GetChaosSequence(int length, double ratio)
        {
            var sequence = string.Empty;
            for (var i = 0; i < length; i++)
            {
                if (Util.Rand.NextDouble() > ratio)
                    sequence += "1 ";
                else
                    sequence += "0 ";
            }

            return sequence;
        }

        private Flame InsertMotion(Flame flame, FlameConfig flameConfig)
        {
            var doc = XDocument.Load(flame.GenomePath);

            foreach (var n in doc.Descendants("flame"))
            {
                foreach (var xform in doc.Descendants("xform"))
                {
                    if (Util.Rand.NextDouble() > flameConfig.MotionDensity) continue;
                    var attr = xform.Attributes().Where(a => double.TryParse(a.Value, out var irrelevant) && (allVars.Any(v => a.Name.LocalName == v) || a.Name == "color")).OrderBy(a => Util.Rand.Next()).LastOrDefault();
                    if (attr == null) continue;
                    xform.Add(CreateMotionElement(xform, attr.Name.LocalName));
                }

                foreach (var xform in doc.Descendants("finalxform"))
                {
                    if (Util.Rand.NextDouble() > flameConfig.MotionDensity) continue;
                    var attr = xform.Attributes().Where(a => double.TryParse(a.Value, out var irrelevant) && (allVars.Any(v => a.Name.LocalName == v) || a.Name == "color")).OrderBy(a => Util.Rand.Next()).LastOrDefault();
                    if (attr == null) continue;
                    xform.Add(CreateMotionElement(xform, attr.Name.LocalName));
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
            var motionFunctions = new List<string>() { "sin", "sin", "triangle", "sin", "hill" };
            var motionFrequencies = new List<string>() { "1", "1", "2", "4" };

            var motionAttrName = name;
            var motionAttr = new XAttribute(motionAttrName, ((Util.Rand.NextDouble() / 2) + 0.5).ToString().Substring(0, 3));
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
