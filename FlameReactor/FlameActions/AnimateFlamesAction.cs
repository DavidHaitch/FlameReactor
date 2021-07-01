using FlameReactor.DB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor.FlameActions
{
    class AnimateFlamesAction : FlameAction
    {
        public AnimateFlamesAction(Action<RenderStepEventArgs> stepEvent) : base(stepEvent) { }
        public override async Task<Flame[]> Act(FlameConfig flameConfig, params Flame[] flames)
        {
            for(var i = 0; i < flames.Length; i++)
            {
                flames[i] = await AnimateFlame(flameConfig, flames[i]);
            }

            return flames;
        }

        private async Task<Flame> AnimateFlame(FlameConfig flameConfig, Flame flame)
        {
            var animationDir = Path.Combine(EnvironmentPaths.FlamePoolPath, flame.Name);
            var animationFlame = Path.Combine(EnvironmentPaths.FlamePoolPath, flame.Name, flame.Name + "_anim.flame");
            Directory.CreateDirectory(animationDir);
            StepEvent(new RenderStepEventArgs("Sequencing", flame.ImagePathWeb, 17));

            var sequenceProcess = await Util.RunProcess(EnvironmentPaths.EmberGenomePath, new[] { "--sequence=" + flame.GenomePath, "--loops=1", "--loopframes=" + flameConfig.LoopFrames, "--noedits"}, animationFlame);
            sequenceProcess.WaitForExit();

            if (sequenceProcess.ExitCode != 0) return null;
            var animationProcess = await Util.RunProcess(EnvironmentPaths.EmberAnimatePath,
                new[] { "--demax=4", "--supersample="+flameConfig.Supersample, "--ts="+flameConfig.TS, "--opencl", "--in=" + animationFlame, "--sp", "--quality=" + flameConfig.Quality,
                "--hs=" + flameConfig.ResolutionMultiplier,
                "--ws=" + flameConfig.ResolutionMultiplier,
                "--ss=" + flameConfig.ResolutionMultiplier});
            var latestCreationTime = DateTime.Now;
            var startTime = DateTime.Now;
            var frameTimes = new List<double>();
            var totalFrames = flameConfig.LoopFrames; //Copy LoopFrames in case it changes during the render.
            while (!animationProcess.HasExited)
            {
                try
                {
                    DirectoryInfo info = new DirectoryInfo(animationDir);
                    var newestFile = info.GetFiles().Where(f => f.FullName.EndsWith(".png")).OrderByDescending(p => p.CreationTime).FirstOrDefault();
                    if (newestFile != null && newestFile.CreationTime > latestCreationTime)
                    {
                        frameTimes.Add((newestFile.CreationTime - latestCreationTime).TotalMilliseconds);
                        if (frameTimes.Count > 32) frameTimes.RemoveAt(0);
                        latestCreationTime = newestFile.CreationTime;

                        var frameNumberName = newestFile.Name.Replace(".png", "");
                        var frameNumber = Convert.ToDouble(frameNumberName);
                        var perFrameTime = TimeSpan.FromMilliseconds(frameTimes.Average());
                        var timeRemaining = perFrameTime * (totalFrames - frameNumber);
                        var percentDone = 20;
                        int animDone = (int)((frameNumber / totalFrames) * 80);
                        percentDone += (animDone);
                        StepEvent(new RenderStepEventArgs("Animating - " + timeRemaining.Minutes + "m " + timeRemaining.Seconds + "s remaining", newestFile.Name, percentDone));
                    }
                   // await Task.Delay(1000);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }


            animationProcess.WaitForExit();
            var exitCode = animationProcess.ExitCode;
            if (exitCode == 0)
            {
                StepEvent(new RenderStepEventArgs("Encoding", "", 100));
                var frameCount = Math.Floor(Math.Log10(totalFrames)) + 1;
                //(await Util.RunProcess(EnvironmentPaths.FFMpegPath,
                //new[] {
                //    "-framerate", "30",
                //    "-i", animationDir + "/%0"+ frameCount + "d.png",
                //    "-vcodec", "h264_nvenc",
                //    "-preset", "p6",
                //    "-pix_fmt", "yuv420p",
                //    "-cq", "20",
                //    animationDir + "/" + flame.Name + ".mp4" })).WaitForExit();
                (await Util.RunProcess(EnvironmentPaths.FFMpegPath,
                new[] {
                    "-framerate", "30",
                    "-i", animationDir +"/%0"+ frameCount + "d.png",
                    "-vcodec", "libx264",
                    "-pix_fmt", "yuv420p",
                    "-crf", "24",
                    "-preset", "slow",
                    animationDir + "/" + flame.Name + ".mp4" })).WaitForExit();
            }

            foreach (var f in Directory.GetFiles(animationDir).Where(x => x.EndsWith(".png")))
            {
                File.Delete(f);
            }

            flame.Update();
            return flame;
        }
    }
}
