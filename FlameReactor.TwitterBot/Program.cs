using FlameReactor.DB;
using FlameReactor.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FlameReactor.TwitterBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var startup = new Startup();

            var firstLines = new List<string>()
            {
                "This is $flameName, child of $p1Name and $p2Name.",
                "Meet $flameName, child of $p1Name and $p2Name.",
                "$p1Name and $p2Name mated to produce $flameName."
            };

            var secondLines = new List<string>()
            {
                "They are a Generation $g fractal.",
                "They are the product of $g generations of breeding."
            };

            var thirdLines = new List<string>()
            {
                "Please welcome them to the world.",
                "They are excited to meet you.",
                "They are a bit shy.",
                "They are famously lustful.",
                "They'd like to go to Mars someday.",
                "They like dusty fractals.",
                "They like geometric fractals.",
                "They like sharp fractals.",
                "They like blurry fractals.",
                "They like simple fractals.",
                "They like fancy fractals.",
                "They like fast fractals.",
                "They like slow fractals.",
                "They want to know what you had for dinner.",
                "Their favorite color hasn't been invented yet.",
                "They are quite curious.",
                "Their favorite color is blue.",
                "Their favorite color is green.",
                "Their favorite color is red.",
                "They have a lot to say.",
                "$flameName prefers dogs to cats.",
                "$flameName prefers cats to dogs.",
                "We don't quite know what to do with this one.",
                "$flameName is especially proud of their patterns",
                "$flameName wishes they had more colors.",
                "$flameName wonders if they're as good as their parents."
            };

            var animationLines = new List<string>()
            {
                "$flameName dances for you.",
                "$flameName wiggles.",
                "$flameName twitches.",
                "$flameName spins.",
                "$flameName flows.",
                "$flameName moves.",
                "The rotation of $flameName.",
                "$flameName's possibility space.",
                "$flameName in motion.",
                "$flameName in flight."
            };

            var hashtags = new List<string>()
            {
                "#generativeArt",
                "#fractalArt",
                "#fractal",
                "#fractalFlame",
                "#artBot"
            };

            var callToAction = "\nIf this flame is beautiful, ❤️ or RT to improve its chances for future breedings.\n";
            var fallbackHashtags = "#$flameName #$p1Name #$p2Name";
            var twitterService = startup.Provider.GetRequiredService<TwitterService>();
            var mastodonService = startup.Provider.GetRequiredService<MastodonService>();
            var ember = new EmberService("Flames/Pool", new DbContextOptionsBuilder<FlameReactorContext>().UseSqlite(@"Data Source=.\FlameReactor.sqlite").Options);
            //ember.FlameConfig.LoopFrames = 600;
            //ember.FlameConfig.GenomeTries = 100;
            //ember.FlameConfig.MaxDisplacement = 10.0;
            //ember.FlameConfig.CenterMagnetism = 0.5;
            //ember.FlameConfig.Quality = 1500;
            //ember.FlameConfig.MaxTransforms = 10;
            //ember.FlameConfig.MinScale = 150;
            //ember.FlameConfig.MaxScale = 5000;
            //ember.FlameConfig.RenderResolutionMultiplier = 1.5;
            //ember.FlameConfig.ResolutionMultiplier = 1.5;
            //ember.FlameConfig.MotionDensity = 0.1;
            //ember.FlameConfig.MutationChance = 0.65;
            //ember.FlameConfig.AlternateSetpoint = 0.033;
            //ember.FlameConfig.UnionSetpoint = 0.8;
            //ember.FlameConfig.SelectionInstability = 150;
            //ember.FlameConfig.AnimationDensity = 0.5;
            //ember.FlameConfig.OrbitDensity = 0.25;
            //ember.FlameConfig.MaxBlur = 1.0;
            //ember.FlameConfig.TS = 100;
            //ember.SaveConfig();
            try
            {
                //RectifyTweetedFlames(ember, twitterService).Wait();
                Console.Write("Updating flame ratings...");
                UpdateFlameRatingsFromTweetsAsync(ember, twitterService).Wait();
                UpdateFlameRatingsFromTootsAsync(ember, mastodonService).Wait();
                UpdateFlameRatings(ember);
                Console.WriteLine(" Done");
                var flame = ember.Breed();
                flame.Wait();
                var rand = new Random((int)DateTime.Now.Ticks);
                var firstLine = formatFlameDesc(firstLines[rand.Next(firstLines.Count)], flame.Result);
                var secondLine = formatFlameDesc(secondLines[rand.Next(secondLines.Count)], flame.Result);
                var thirdLine = formatFlameDesc(thirdLines[rand.Next(thirdLines.Count)], flame.Result);
                var hashtag = formatFlameDesc(hashtags[rand.Next(hashtags.Count)], flame.Result);
                var status = firstLine + "\n"
                    + secondLine + "\n"
                    + thirdLine + "\n"
                    + formatFlameDesc(callToAction, flame.Result)
                    + formatFlameDesc(hashtag, flame.Result).Replace("-", "_");
                if (status.Length > 275)
                {
                    status = firstLine + "\n"
                    + secondLine + "\n"
                    + formatFlameDesc(callToAction, flame.Result)
                    + formatFlameDesc(hashtag, flame.Result).Replace("-", "_");
                }
                if (status.Length > 275)
                {
                    status = firstLine + "\n" + secondLine + "\n" + formatFlameDesc(callToAction, flame.Result) + formatFlameDesc(fallbackHashtags, flame.Result).Replace("-", "_");
                }
                var toot = mastodonService.UploadSingleImageAsync(status, flame.Result.ImagePath);
                toot.Wait();
                if (toot.Result != null)
                {
                    ember.AddTweetRecordToFlame(flame.Result, new TweetRecord() { ID = (ulong)toot.Result.Id, Faves = 0, Retweets = 0, Source = "Mastodon" });
                }

                var tweet = twitterService.UploadSingleImageAsync(status, flame.Result.ImagePath);
                tweet.Wait();
                if (tweet.Result != null)
                {
                    ember.AddTweetRecordToFlame(flame.Result, new TweetRecord() { ID = tweet.Result.StatusID, Faves = 0, Retweets = 0, Source = "Twitter" });
                }

                var animated = ember.Animate(flame.Result);
                animated.Wait();
                var animationLine = formatFlameDesc(animationLines[rand.Next(animationLines.Count)], flame.Result);
                var videoToot = mastodonService.UploadVideoAsync(toot.Result, animationLine + "\n" + formatFlameDesc(callToAction, flame.Result) + formatFlameDesc(hashtag, flame.Result).Replace("-", "_"), animated.Result.VideoPath);
                videoToot.Wait();
                if (videoToot.Result != null)
                {
                    ember.AddTweetRecordToFlame(flame.Result, new TweetRecord() { ID = (ulong)videoToot.Result.Id, Faves = 0, Retweets = 0, Source = "Mastodon" });
                }

                var videoTweet = twitterService.UploadVideoAsync(tweet.Result, animationLine + "\n" + formatFlameDesc(callToAction, flame.Result) + formatFlameDesc(hashtag, flame.Result).Replace("-", "_"), animated.Result.VideoPath);
                videoTweet.Wait();
                if (videoTweet.Result != null)
                {
                    ember.AddTweetRecordToFlame(flame.Result, new TweetRecord() { ID = videoTweet.Result.StatusID, Faves = 0, Retweets = 0, Source = "Twitter" });
                }
            }
            catch (Exception ex)
            {

                File.AppendAllText("error.log", "--------------" + "\n");
                File.AppendAllText("error.log", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\n");
                File.AppendAllText("error.log", ex.Message + "\n");
                File.AppendAllText("error.log", ex.StackTrace + "\n");
                File.AppendAllText("error.log", "--------------" + "\n");
            }
        }

        private static async Task RectifyTweetedFlames(EmberService es, TwitterService ts)
        {
            var rand = new Random(DateTime.Now.Millisecond);
            var flames = es.GetUntweetedFlames().OrderBy(f => rand.Next()).Take(10);
            foreach (var flame in flames)
            {
                Thread.Sleep(1000);
                var tweetId = await ts.GetTweetIdForFlameName(flame.DisplayName + " " + flame.Birth.Parents[0].DisplayName + " FlameReactorBot");
                if (tweetId == 0) continue;
                es.AddTweetRecordToFlame(flame, new TweetRecord() { ID = tweetId, Faves = 0, Retweets = 0 });
            }
        }

        private static async Task UpdateFlameRatingsFromTootsAsync(EmberService es, MastodonService ms)
        {
            var tweets = es.GetTweetRecords().Where(tr => tr.Source == "Mastodon").OrderByDescending(t => t.ID).Take(250);
            var tasks = tweets.Select(t =>
            {
                return Task.Run(async () =>
                {
                    es.UpdateTweetRecord(await ms.GetCountsForTootAsync(t));
                });
            });

            await Task.WhenAll(tasks);
        }

        private static async Task UpdateFlameRatingsFromTweetsAsync(EmberService es, TwitterService ts)
        {
            if (await ts.GetStatusRequestsRemaining() < 250) return;

            var tweets = es.GetTweetRecords().Where(tr => tr.Source == null || tr.Source == "Twitter").OrderByDescending(t => t.ID).Take(250);
            var tasks = tweets.Select(t =>
            {
                return Task.Run(async () =>
                {
                    es.UpdateTweetRecord(await ts.GetCountsForTweetAsync(t));
                });
            });

            await Task.WhenAll(tasks);

            var tweetsByOwner = es.GetTweetRecords().GroupBy(t => t.Owner);
            foreach (var owner in tweetsByOwner)
            {
                var rating = owner.Sum(x => x.Faves) + owner.Sum(x => x.Retweets);
                es.SetFlameRating(owner.Key, rating);
            }
        }

        private static void UpdateFlameRatings(EmberService es)
        {
            var tweetsByOwner = es.GetTweetRecords().GroupBy(t => t.Owner);
            foreach (var owner in tweetsByOwner)
            {
                var rating = owner.Sum(x => x.Faves) + owner.Sum(x => x.Retweets);
                es.SetFlameRating(owner.Key, rating);
            }
        }

        private static string formatFlameDesc(string template, Flame flame)
        {
            template = template.Replace("$flameName", flame.DisplayName);
            template = template.Replace("$p1Name", flame.Birth.Parents[0].DisplayName);
            template = template.Replace("$p2Name", flame.Birth.Parents[1].DisplayName);
            template = template.Replace("$g", flame.Generation.ToString());
            return template;
        }
    }
}
