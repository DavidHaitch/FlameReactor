using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor
{
    internal static class Util
    {
        internal static Random Rand
        {
            get
            {
                if (rand == null)
                {
                    rand = new Random((int)DateTime.Now.Ticks);
                }

                return rand;
            }
        }
        private static Random rand;


        internal static async Task<Process> RunProcess(string fileName, string[] arguments, string outputFile = null)
        {
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            arguments.ToList().ForEach(a => processStartInfo.ArgumentList.Add(a));
            var process = new Process
            {
                StartInfo = processStartInfo
            };
            process.Start();
            process.BeginOutputReadLine();

            string filedata = "";
            if (outputFile != null)
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    filedata += e.Data;
                };
            }


            if (outputFile != null)
            {
                process.WaitForExit();
                File.WriteAllText(outputFile, filedata);
            }

            return process;
        }
    }
}
