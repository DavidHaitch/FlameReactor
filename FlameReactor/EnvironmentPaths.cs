using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor
{
    internal static class EnvironmentPaths
    {
        internal static string FlamePoolPath { get; set; }
        internal static string FractoriumDirectory { get { return Path.Combine(new[] { AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Fractorium", "Win" }); } }
        internal static string EmberAnimatePath { get { return Path.Combine(FractoriumDirectory, "EmberAnimate.exe"); } }
        internal static string EmberGenomePath { get { return Path.Combine(FractoriumDirectory, "EmberGenome.exe"); } }
        internal static string EmberRenderPath { get { return Path.Combine(FractoriumDirectory, "EmberRender.exe"); } }
        internal static string FFMpegPath { get { return Path.Combine(new[] { AppDomain.CurrentDomain.BaseDirectory, "Binaries", "ffmpeg", "bin", "ffmpeg.exe" }); } }
    }
}
