using FlameReactor.DB;
using FlameReactor.DB.Models;
using FlameReactor.FlameActions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using System.Xml.Linq;

namespace FlameReactor
{
    public class EmberService
    {
        public FlameConfig FlameConfig;
        public event EventHandler<RenderStepEventArgs> RenderStep;
        public event EventHandler<RenderCompleteEventArgs> RenderComplete;

        private AnimateFlamesAction animateFlames;
        private CrossFlamesAction crossFlames;
        private FixColorPalettesAction fixColorPalettes;
        private MutateFlamesAction mutateFlames;
        private RectifyGenomesAction rectifyGenomes;
        private RenderFlamesAction renderFlames;
        public EmberService(string FlameDir)
        {
            EnvironmentPaths.FlamePoolPath = FlameDir;
            FlameConfig = new FlameConfig();
            animateFlames = new AnimateFlamesAction(OnRaiseRenderStepEvent);
            crossFlames = new CrossFlamesAction(OnRaiseRenderStepEvent);
            fixColorPalettes = new FixColorPalettesAction(OnRaiseRenderStepEvent);
            mutateFlames = new MutateFlamesAction(OnRaiseRenderStepEvent);
            rectifyGenomes = new RectifyGenomesAction(OnRaiseRenderStepEvent);
            renderFlames = new RenderFlamesAction(OnRaiseRenderStepEvent);
            BuildFlames(FlameDir).Wait();
        }

        public List<Flame> GetRandomFlames(int count)
        {
            using (var db = new FlameReactorContext())
            {
                return db.Flames.ToList().OrderBy(f => Util.Rand.Next()).Take(count).ToList();
            }
        }

        public List<Flame> GetEligibleFlames(int count)
        {
            var r = Util.Rand.NextDouble();
            var flamesByEligibility = new List<Flame>().AsEnumerable();
            using (var db = new FlameReactorContext())
            {
                flamesByEligibility = db.Flames.ToList().OrderBy(f => Util.Rand.Next()).OrderByDescending(f => f.Rating - (Math.Pow(f.Promiscuity, FlameConfig.PromiscuityDecay)) + Util.Rand.Next(-5, 5));
            }

            if (r < 0.8)
                return flamesByEligibility.Take(count).ToList();
            else if (r < 0.9)
                return flamesByEligibility.Reverse().Take(count).ToList();
            else
                return GetRandomFlames(count);
        }

        public Flame GetMostRecentFlame()
        {
            using (var db = new FlameReactorContext())
            {
                return db.Flames.ToList().OrderByDescending(x => (new FileInfo(x.GenomePath).CreationTime)).First();
            }
        }

        public async Task PopulatePool()
        {
            var flameName = Path.Combine(EnvironmentPaths.FlamePoolPath, "seed_" + Guid.NewGuid().ToString() + ".flame");
            await Util.RunProcess(EnvironmentPaths.EmberGenomePath,
            new[] { "--tries=" + FlameConfig.GenomeTries, "--debug" },
            flameName);
            var flame = new Flame(flameName);
            await rectifyGenomes.Act(FlameConfig, flame);
            await renderFlames.Act(FlameConfig, flame);
            flame.Update();
            using (var db = new FlameReactorContext())
            {
                db.Flames.Add(flame);
                db.SaveChanges();
            }
        }

        public bool Vote(string IP, Flame flame, int adjustment, bool force = false)
        {
            using (var db = new FlameReactorContext())
            {
                var f = db.Flames.FirstOrDefault(f => f.ID == flame.ID);
                if (f == null) throw new Exception("Tried to vote on nonexistent flame: " + flame.ID);

                if (!force && db.Votes.Any(v => v.IPAddress == IP && v.FlameId == flame.ID)) return false;
                if (!force) db.Votes.Add(new Vote() { FlameId = flame.ID, IPAddress = IP, Adjustment = adjustment });
                f.Rating += adjustment;
                db.SaveChanges();
                return true;
            }
        }

        public async Task<Flame> Breed()
        {
            var rand = GetRandomFlames(2);
            return await Breed(rand[0], rand[1]);
        }

        public async Task<Flame> Breed(Flame flame1, Flame flame2)
        {
            await Task.Delay(100);
            using (var db = new FlameReactorContext())
            {
                var f1 = db.Flames.FirstOrDefault(f => f.ID == flame1.ID);
                var f2 = db.Flames.FirstOrDefault(f => f.ID == flame2.ID);
                var flame = await crossFlames.Act(FlameConfig, f1, f2);
                db.Flames.AddRange(flame);
                db.Flames.Update(f1);
                db.Flames.Update(f2);
                db.SaveChanges();
                for (int i = 0; i < flame.Length; i++)
                {
                    flame[i].Birth.ChildID = flame[i].ID;
                }

                db.SaveChanges();

                flame = await mutateFlames.Act(FlameConfig, flame);
                flame = await mutateFlames.Act(FlameConfig, flame);
                flame = await mutateFlames.Act(FlameConfig, flame);
                //flame = await fixColorPalettes.Act(FlameConfig, flame);
                flame = await rectifyGenomes.Act(FlameConfig, flame);
                flame = await renderFlames.Act(FlameConfig, flame);
                if (flame.Any(f => f == null)) throw new Exception("Breed error");


                db.SaveChanges();

                var f = flame.First();
                //OnRaiseRenderCompleteEvent(new RenderCompleteEventArgs(f.VideoPathWeb));
                return f;
            }
        }

        public async Task<Flame> Animate(Flame fl)
        {
            await Task.Delay(100);
            using (var db = new FlameReactorContext())
            {
                var flame = await animateFlames.Act(FlameConfig, fl);
                if (flame.Any(f => f == null)) throw new Exception("Breed error");
                db.SaveChanges();
                var f = flame.First();
                OnRaiseRenderCompleteEvent(new RenderCompleteEventArgs(f.VideoPathWeb));
                return f;
            }
        }

        public void ChangeFlame(Flame flame)
        {
            UpdateFlame(flame);
        }

        private Flame UpdateFlame(Flame flame)
        {
            using (var db = new FlameReactorContext())
            {
                var f = db.Flames.FirstOrDefault(f => f.ID == flame.ID);
                if (f != null)
                {
                    f.Update();
                    f.DisplayName = flame.DisplayName;
                    f.Generation = flame.Generation;
                    f.Promiscuity = flame.Promiscuity;
                    f.Rating = flame.Rating;
                }
                else
                {
                    db.Flames.Add(flame);
                }

                db.SaveChanges();
            }

            return flame;
        }

        private async Task BuildFlames(string FlameDir)
        {
            var startingFlameCount = 0;
            using (var db = new FlameReactorContext())
            {
                foreach (var f in Directory.GetFiles(FlameDir).Where(x => x.EndsWith(".flame")))
                {
                    var tempFlame = new Flame(f);
                    if (!db.Flames.Any(f_ => f_.Name == tempFlame.Name))
                    {
                        db.Flames.Add(tempFlame);
                    }
                }

                db.SaveChanges();
                startingFlameCount = db.Flames.Count();
            }
            var addedFlames = 0;
            var minFlockSize = 0;
            var batchSize = 4;
            while (startingFlameCount + addedFlames < minFlockSize)
            {
                var processes = startingFlameCount + addedFlames + batchSize > minFlockSize ? minFlockSize - (startingFlameCount + addedFlames) : batchSize;
                var tasks = new Task[processes];

                for (int i = 0; i < processes; i++)
                {
                    tasks[i] = Task.Run(() => PopulatePool());
                }

                Task.WaitAll(tasks);
                addedFlames += processes;
            }

            await RepairFlock();
        }

        private async Task RepairFlock()
        {
            try
            {
                using (var db = new FlameReactorContext())
                {

                    var missingFlames = new List<Flame>();
                    var allFlames = db.Flames.ToList();
                    var names = File.ReadAllLines("./mysteryNames.txt").ToList();
                    foreach (var flame in allFlames)
                    {
                        if (!File.Exists(flame.GenomePath)) missingFlames.Add(flame);
                        if (!File.Exists(flame.ImagePath)) flame.ImagePath = "";
                        if (!File.Exists(flame.VideoPath)) flame.VideoPath = "";
                        if (flame.DisplayName == flame.Name)
                        {
                            var name = names[Util.Rand.Next(names.Count())];
                            flame.DisplayName = name.First().ToString().ToUpper() + name.Substring(1).ToLower();
                        }
                    }

                    missingFlames.ForEach(f => db.Flames.Remove(f));
                    db.SaveChanges();
                    allFlames = db.Flames.ToList();

                    var imagelessFlames = allFlames.Where(f => !File.Exists(f.ImagePath)).ToArray();
                    var videolessFlames = allFlames.Where(f => !File.Exists(f.VideoPath)).ToArray();

                    imagelessFlames = await rectifyGenomes.Act(FlameConfig, imagelessFlames);
                    imagelessFlames = await renderFlames.Act(FlameConfig, imagelessFlames);

                    //videolessFlames = await rectifyGenomes.Act(FlameConfig, videolessFlames);
                    //videolessFlames = await animateFlames.Act(FlameConfig, videolessFlames);

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        internal virtual void OnRaiseRenderStepEvent(RenderStepEventArgs e)
        {
            Console.WriteLine(e.StepData);
            var raiseEvent = RenderStep;
            if (raiseEvent != null)
            {
                raiseEvent(this, e);
            }
        }

        protected virtual void OnRaiseRenderCompleteEvent(RenderCompleteEventArgs e)
        {
            var raiseEvent = RenderComplete;
            if (raiseEvent != null)
            {
                raiseEvent(this, e);
            }
        }
    }
}
