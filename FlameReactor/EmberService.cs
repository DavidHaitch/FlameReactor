using FlameReactor.DB;
using FlameReactor.DB.Models;
using FlameReactor.FlameActions;
using Microsoft.EntityFrameworkCore;
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
        private DbContextOptions<FlameReactorContext> _dbContextOptions;
        public EmberService(string FlameDir, DbContextOptions<FlameReactorContext> dbContextOptions, bool skipBuild = false)
        {
            EnvironmentPaths.FlamePoolPath = FlameDir;
            _dbContextOptions = dbContextOptions;
            var configInit = false;
            if (File.Exists("./flameConfig.json"))
            {
                try
                {
                    FlameConfig = JsonSerializer.Deserialize<FlameConfig>(File.ReadAllText("./flameConfig.json"));
                    configInit = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load flameConfig.json - " + ex.Message);
                }
            }
            if (!configInit)
            {
                Console.WriteLine("No flameConfig.json found, using defaults.");
                FlameConfig = new FlameConfig();
                SaveConfig();
            }

            animateFlames = new AnimateFlamesAction(OnRaiseRenderStepEvent);
            crossFlames = new CrossFlamesAction(OnRaiseRenderStepEvent);
            fixColorPalettes = new FixColorPalettesAction(OnRaiseRenderStepEvent);
            mutateFlames = new MutateFlamesAction(OnRaiseRenderStepEvent);
            rectifyGenomes = new RectifyGenomesAction(OnRaiseRenderStepEvent);
            renderFlames = new RenderFlamesAction(OnRaiseRenderStepEvent);

            if (!skipBuild)
            {
                BuildFlames(FlameDir).Wait();
            }
        }

        public void SaveConfig()
        {
            File.WriteAllText("./flameConfig.json", JsonSerializer.Serialize(FlameConfig, new JsonSerializerOptions { WriteIndented = true }));
        }

        public List<Flame> GetRandomFlames(int count)
        {
            using var dbContext = new FlameReactorContext(_dbContextOptions);
            return dbContext.Flames.ToList().Where(f => !f.Dead && f.Generation < 5).OrderBy(f => Util.Rand.Next()).Take(count).ToList();
        }

        public List<Flame> GetEligibleFlames(int count, Flame currentFlame = null)
        {
            var rand = new Random();
            var r = rand.NextDouble();
            using var dbContext = new FlameReactorContext(_dbContextOptions);
            var minRating = dbContext.Flames.Min(f => f.Rating);
            var maxRating = dbContext.Flames.Max(f => f.Rating);
            var minPromiscuity = dbContext.Flames.Min(f => f.Promiscuity);
            var maxPromiscuity = dbContext.Flames.Max(f => f.Promiscuity);

            var flamesByEligibility = dbContext.Flames
                .Include(f => f.Birth).ThenInclude(b => b.Parents)
                .ThenInclude(p => p.Birth).ThenInclude(b => b.Parents).ToList();
            if (currentFlame != null)
            {
                var current = dbContext.Flames
                    .Include(f => f.Birth).ThenInclude(b => b.Parents)
                    .ThenInclude(p => p.Birth).ThenInclude(b => b.Parents)
                    .First(f => f.ID == currentFlame.ID);
                flamesByEligibility = flamesByEligibility.Where(f => !f.Dead && f.ID != current.ID
                   && f.Generation <= current.Generation
                   && (current.Birth == null || (!current.Birth.Parents.Any(f1 => f1.ID == f.ID)))
                   && (current.Birth == null || current.Birth.Parents.All(p => p.Birth == null || (!p.Birth.Parents.Any(f2 => f2.ID == f.ID))))).ToList();
            }

            flamesByEligibility = flamesByEligibility.OrderBy(f => Util.Rand.Next()).ToList();
            flamesByEligibility = flamesByEligibility.OrderByDescending(f => f.Rating - (Util.Rand.Next(0, (int)FlameConfig.SelectionInstability))).ToList();
            return flamesByEligibility.Take(count).ToList();
        }

        public Flame GetMostRecentFlame()
        {
            using var dbContext = new FlameReactorContext(_dbContextOptions);
            return dbContext.Flames.ToList().OrderByDescending(x => x.Generation).First();
        }

        public async Task PopulatePool()
        {
            using var dbContext = new FlameReactorContext(_dbContextOptions);

            var flameName = Path.Combine(EnvironmentPaths.FlamePoolPath, "seed_" + Guid.NewGuid().ToString() + ".flame");
            await Util.RunProcess(EnvironmentPaths.EmberGenomePath,
            new[] { "--tries=" + FlameConfig.GenomeTries, "--debug", "--opencl", "--cvbreeding" },
            flameName);
            var flame = new Flame(flameName);
            await rectifyGenomes.Act(FlameConfig, flame);
            await renderFlames.Act(FlameConfig, flame);
            flame.Update();

            dbContext.Flames.Add(flame);
            dbContext.SaveChanges();
        }

        public List<Flame> GetUntweetedFlames()
        {
            using var dbContext = new FlameReactorContext(_dbContextOptions);
            return dbContext.Flames.Where(f => f.Tweets.Count() == 0 && f.Generation > 0).Include(f => f.Birth).ThenInclude(b => b.Parents).ToList();
        }

        public List<TweetRecord> GetTweetRecords()
        {
            using var dbContext = new FlameReactorContext(_dbContextOptions);
            return dbContext.TweetRecord.Include(tr => tr.Owner).ToList();
        }

        public TweetRecord UpdateTweetRecord(TweetRecord tweetRecord)
        {
            if (tweetRecord == null) return null;
            using var dbContext = new FlameReactorContext(_dbContextOptions);
            var tr = dbContext.TweetRecord.FirstOrDefault(t => t.ID == tweetRecord.ID);
            tr.Faves = tweetRecord.Faves;
            tr.Retweets = tweetRecord.Retweets;
            dbContext.SaveChanges();
            return tr;
        }
        public void AddTweetRecordToFlame(Flame flame, TweetRecord tweetRecord)
        {
            using var dbContext = new FlameReactorContext(_dbContextOptions);
            var f = dbContext.Flames.FirstOrDefault(f => f.ID == flame.ID);
            tweetRecord.Owner = f;
            dbContext.TweetRecord.Add(tweetRecord);
            dbContext.SaveChanges();
        }

        public void SetFlameRating(Flame flame, int rating)
        {
            using var dbContext = new FlameReactorContext(_dbContextOptions);
            var f = dbContext.Flames.FirstOrDefault(f => f.ID == flame.ID);
            if (f == null) return;
            f.Rating = rating;
            dbContext.SaveChanges();
        }

        public void SetFlameRating(int flameId, int rating)
        {
            using var dbContext = new FlameReactorContext(_dbContextOptions);
            var f = dbContext.Flames.FirstOrDefault(f => f.ID == flameId);
            if (f == null) return;
            f.Rating = rating;
            dbContext.SaveChanges();
        }

        public bool Vote(string IP, Flame flame, int adjustment, bool force = false)
        {
            using var dbContext = new FlameReactorContext(_dbContextOptions);
            var f = dbContext.Flames.Include(f => f.Birth).FirstOrDefault(f => f.ID == flame.ID);
            if (f == null) throw new Exception("Tried to vote on nonexistent flame: " + flame.ID);

            if (!force && dbContext.Votes.Any(v => v.IPAddress == IP && v.FlameId == flame.ID)) return false;
            if (!force) dbContext.Votes.Add(new Vote() { FlameId = flame.ID, IPAddress = IP, Adjustment = adjustment });
            f.Rating += adjustment;
            Console.WriteLine("Voting on " + f.DisplayName + " by " + adjustment);
            if (Math.Abs(adjustment / 2) > 0 && f.BirthID != null)
            {
                var parents = dbContext.Flames.Include(x => x.Breedings).ToList().Where(x => x.Breedings.Any(b => b.ID == f.BirthID)).ToList();
                parents.ForEach(p => Vote(IP, p, adjustment / 2, true));
                dbContext.Flames.UpdateRange(parents);
            }

            dbContext.SaveChanges();
            return true;
        }

        public async Task<Flame> Breed()
        {
            var parent1 = GetEligibleFlames(1).First();
            var parent2 = GetEligibleFlames(1, parent1).First();
            return await Breed(parent1, parent2);
        }

        public async Task<Flame> Breed(Flame flame1, Flame flame2)
        {
            await Task.Delay(100);
            using var dbContext = new FlameReactorContext(_dbContextOptions);
            var f1 = dbContext.Flames.FirstOrDefault(f => f.ID == flame1.ID);
            var f2 = dbContext.Flames.FirstOrDefault(f => f.ID == flame2.ID);
            var flame = await crossFlames.Act(FlameConfig, f1, f2);
            dbContext.Flames.AddRange(flame);
            dbContext.Flames.Update(f1);
            dbContext.Flames.Update(f2);
            dbContext.SaveChanges();

            for (int i = 0; i < flame.Length; i++)
            {
                flame[i].Birth.ChildID = flame[i].ID;
            }

            dbContext.SaveChanges();

            while (Util.Rand.NextDouble() < FlameConfig.MutationChance)
            {
                flame = await mutateFlames.Act(FlameConfig, flame);
            }

            //flame = await fixColorPalettes.Act(FlameConfig, flame);
            flame = await rectifyGenomes.Act(FlameConfig, flame);
            flame = await renderFlames.Act(FlameConfig, flame);
            if (flame.Any(f => f == null)) throw new Exception("Breed error");


            dbContext.SaveChanges();

            var f = flame.First();
            //OnRaiseRenderCompleteEvent(new RenderCompleteEventArgs(f.VideoPathWeb));
            return f;
        }

        public async Task<Flame> Animate(Flame fl)
        {
            await Task.Delay(100);
            using (var db = new FlameReactorContext())
            {
                var flame = await animateFlames.Act(FlameConfig, db.Flames.FirstOrDefault(f => f.ID == fl.ID));
                if (flame.Any(f => f == null)) throw new Exception("Animation error");
                var f = flame.First();
                db.Flames.Update(f);
                db.SaveChanges();
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
            using var dbContext = new FlameReactorContext(_dbContextOptions);
            var f = dbContext.Flames.FirstOrDefault(f => f.ID == flame.ID);
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
                dbContext.Flames.Add(flame);
            }

            dbContext.SaveChanges();

            return flame;
        }

        private async Task BuildFlames(string FlameDir)
        {
            var startingFlameCount = 0;

            using var dbContext = new FlameReactorContext(_dbContextOptions);
            foreach (var f in Directory.GetFiles(FlameDir).Where(x => x.EndsWith(".flame")))
            {
                if (!dbContext.Flames.Any(f_ => f_.Name == Path.GetFileNameWithoutExtension(f)))
                {
                    var flamePaths = DecomposeFlameFile(f);
                    foreach (var flamePath in flamePaths)
                    {
                        var tempFlame = new Flame(flamePath);
                        Console.WriteLine("Adding new flame: " + flamePath);
                        dbContext.Flames.Add(tempFlame);
                    }
                }
            }

            Console.Write("Scanning flames...  ");
            foreach (var f in dbContext.Flames)
            {
                f.Update();
            }
            Console.WriteLine("Done.");
            dbContext.SaveChanges();
            startingFlameCount = dbContext.Flames.Count(f => !f.Dead);

            var addedFlames = 0;
            var minFlockSize = 5;
            var batchSize = 3;
            if (startingFlameCount < minFlockSize)
            {
                Console.WriteLine($"{minFlockSize - startingFlameCount} flames missing.");
            }
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
                Console.Write("Repairing flock... ");
                //var missingFlames = new List<Flame>();
                using var dbContext = new FlameReactorContext(_dbContextOptions);
                var allFlames = dbContext.Flames.ToList();
                var names = File.ReadAllLines("./mysteryNames.txt").ToList();
                foreach (var flame in allFlames)
                {
                    if (!File.Exists(flame.ImagePath)) flame.ImagePath = "";
                    if (!File.Exists(flame.VideoPath)) flame.VideoPath = "";
                    if (flame.DisplayName == flame.Name)
                    {
                        var name = names[Util.Rand.Next(names.Count())];
                        flame.DisplayName = name.First().ToString().ToUpper() + name.Substring(1).ToLower();
                    }
                }

                var missingFlames = dbContext.Flames.ToList().Where(flame => !flame.Dead && !File.Exists(flame.GenomePath));
                foreach (var f in missingFlames)
                {
                    dbContext.Flames.Remove(f);
                };
                dbContext.SaveChanges();
                allFlames = dbContext.Flames.Where(f => !f.Dead).ToList();

                var imagelessFlames = allFlames.Where(f => !File.Exists(f.ImagePath)).ToArray();
                var videolessFlames = allFlames.Where(f => !File.Exists(f.VideoPath)).ToArray();

                imagelessFlames = await rectifyGenomes.Act(FlameConfig, imagelessFlames);
                imagelessFlames = await renderFlames.Act(FlameConfig, imagelessFlames);

                //videolessFlames = await rectifyGenomes.Act(FlameConfig, videolessFlames);
                //videolessFlames = await animateFlames.Act(FlameConfig, videolessFlames);

                dbContext.SaveChanges();
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private List<string> DecomposeFlameFile(string flamePath)
        {
            var flameFiles = new List<string>();
            XDocument doc;
            try
            {
                doc = XDocument.Load(flamePath);
            }
            catch (XmlException ex)
            {
                Console.WriteLine($"{flamePath} was not valid XML!");
                File.Delete(flamePath);
                return flameFiles;
            }

            var flameNodes = doc.Descendants("flame");
            if (flameNodes.Count() > 1)
            {
                var root = Path.GetDirectoryName(flamePath);
                var baseName = Path.GetFileNameWithoutExtension(flamePath);
                int count = 0;
                foreach (var node in flameNodes)
                {
                    if (node.Attribute("name").Value.EndsWith(" Bars"))
                    {
                        continue;
                    }

                    var filename = Path.Combine(root, $"{baseName}_{count}.flame");
                    node.Save(filename);
                    flameFiles.Add(filename);
                    count++;
                }

                File.Delete(flamePath);
            }
            else
            {
                flameFiles.Add(flamePath);
            }

            return flameFiles;
        }

        internal virtual void OnRaiseRenderStepEvent(RenderStepEventArgs e)
        {
            Console.WriteLine($"{e.StepData} {e.FramePath}");
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
