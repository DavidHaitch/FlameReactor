using FlameReactor.DB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FlameReactor.FlameActions
{
    class RectifyGenomesAction : FlameAction
    {
        public RectifyGenomesAction(Action<RenderStepEventArgs> stepEvent) : base(stepEvent) { }
        public override async Task<Flame[]> Act(FlameConfig flameConfig, params Flame[] flames)
        {
            foreach (var flame in flames)
            {
                RectifyGenome(flameConfig, flame);
            }

            return flames;
        }

        private void RectifyGenome(FlameConfig flameConfig, Flame flame)
        {
            Console.WriteLine("Rectifying " + flame.GenomePath);
            var doc = XDocument.Load(flame.GenomePath);
            foreach (var n in doc.Descendants("flame"))
            {
                n.SetAttributeValue("name", flame.Name);
                n.SetAttributeValue("size", "1280 720");
                n.SetAttributeValue("zoom", "0");
                //n.SetAttributeValue("gamma_threshold", 0.05);
                //n.SetAttributeValue("gamma", 8);
                var scale = n.Attribute("scale").Value;
                if (scale.Contains("e") || Convert.ToDouble(scale) > flameConfig.MaxScale)
                {
                    Console.WriteLine("Coercing scale from " + scale + " to " + flameConfig.MaxScale);
                    n.SetAttributeValue("scale", flameConfig.MaxScale);
                }
                else if (Convert.ToDouble(scale) < flameConfig.MinScale)
                {
                    Console.WriteLine("Coercing scale from " + scale + " to " + flameConfig.MinScale);
                    n.SetAttributeValue("scale", flameConfig.MinScale);
                }

                var center = n.Attribute("center").Value;
                var centerElements = center.Split(" ");

                if (Math.Max(flameConfig.MaxDisplacement, Math.Abs(Convert.ToDouble(centerElements[0]))) > flameConfig.MaxDisplacement)
                {
                    Console.WriteLine("Coercing X displacement from " + centerElements[0] + " to " + flameConfig.MaxDisplacement);
                }

                if (Math.Max(flameConfig.MaxDisplacement, Math.Abs(Convert.ToDouble(centerElements[1]))) > flameConfig.MaxDisplacement)
                {
                    Console.WriteLine("Coercing Y displacement from " + centerElements[1] + " to " + flameConfig.MaxDisplacement);
                }

                var centerX = Math.Clamp(Convert.ToDouble(centerElements[0]), flameConfig.MaxDisplacement * -1, flameConfig.MaxDisplacement);
                var centerY = Math.Clamp(Convert.ToDouble(centerElements[1]), flameConfig.MaxDisplacement * -1, flameConfig.MaxDisplacement);

                if (Math.Abs(Convert.ToDouble(centerElements[0])) < flameConfig.CenterMagnetism)
                {
                    Console.WriteLine($"Magnetising X from {centerElements[0]} to 0.");
                    centerX = 0;
                }

                if (Math.Abs(Convert.ToDouble(centerElements[1])) < flameConfig.CenterMagnetism)
                {
                    Console.WriteLine($"Magnetising Y from {centerElements[1]} to 0.");
                    centerY = 0;
                }

                n.SetAttributeValue("center", centerX + " " + centerY);
            }

            var xforms = doc.Descendants("xform");
            foreach (var final in doc.Descendants("finalxform"))
            {
                xforms.Append(final);
            }

            foreach (var xform in xforms)
            {
                var animate = xform.Attribute("animate") != null ? xform.Attribute("animate").Value : "0";
                xform.SetAttributeValue("animate", Math.Round(Convert.ToDouble(animate)));

                foreach (var attr in xform.Attributes())
                {
                    if (attr.Name.LocalName.Contains("blur"))
                    {
                        if (Convert.ToDouble(attr.Value) > flameConfig.MaxBlur)
                            attr.Value = flameConfig.MaxBlur.ToString();
                    }
                }
            }

            if (xforms.All(x => x.Attribute("animate").Value == "1") || xforms.Count(x => x.Attribute("animate").Value == "1") <= xforms.Count() * flameConfig.AnimationDensity)
            {
                Console.WriteLine("Randomizing bad animation settings.");
                foreach (var n in xforms)
                {
                    n.SetAttributeValue("animate", 0);
                }

                var xformCount = xforms.Count();
                for (var i = 0; i <= xformCount * flameConfig.AnimationDensity; i++)
                {
                    xforms.ElementAt(Util.Rand.Next(0, xformCount)).SetAttributeValue("animate", 1);
                }
            }

            foreach (var n in xforms)
            {
                if (n.Attribute("orbit") == null)
                {
                    n.SetAttributeValue("orbit", 0);
                }
            }

            if (xforms.All(x => x.Attribute("orbit").Value != "0") || xforms.Count(x => x.Attribute("orbit").Value != "0") <= xforms.Count() * flameConfig.OrbitDensity)
            {
                Console.WriteLine("Randomizing bad orbit settings.");
                foreach (var n in xforms)
                {
                    n.SetAttributeValue("orbit", 0);
                }

                var xformCount = xforms.Count();
                for (var i = 0; i <= xformCount * flameConfig.AnimationDensity; i++)
                {
                    if (Util.Rand.NextDouble() < 0.5)
                        xforms.ElementAt(Util.Rand.Next(0, xformCount)).SetAttributeValue("orbit", 1);
                    else
                        xforms.ElementAt(Util.Rand.Next(0, xformCount)).SetAttributeValue("orbit", -1);
                }
            }

            using (var writer = new XmlTextWriter(flame.GenomePath, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                doc.Save(writer);
            }

            Console.WriteLine("Done rectifying " + flame.GenomePath);
        }
    }
}
