using FlameReactor.DB.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
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
                "This is $flameName, offspring of $p1Name and $p2Name.",
                "Meet $flameName, child of $p1Name and $p2Name.",
                "$p1Name and $p2Name mated to produce $flameName."
            };

            var secondLines = new List<string>()
            {
                "They are a Generation $g fractal."
            };

            var thirdLines = new List<string>()
            {
                "Please welcome them to the world.",
                "They are excited to meet you.",
                "They are a bit shy.",
                "$flameName is especially proud of their patterns",
                "$flameName wishes they had more colors."
            };

            var animationLines = new List<string>()
            {
                "$flameName dances for you.",
                "$flameName morphs in delight."
            };

            var hashtags = "#fractal #fractalflame #fractalart #generativeArt #$flameName #$p1Name #$p2Name";
            var twitterService = startup.Provider.GetRequiredService<TwitterService>();
            var ember = new EmberService("Flames/Pool");
            ember.FlameConfig.LoopFrames = 600;
            ember.FlameConfig.GenomeTries = 20;
            ember.FlameConfig.MaxDisplacement = 10;
            ember.FlameConfig.Quality = 1500;
            ember.FlameConfig.RenderResolutionMultiplier = 1.5;
            ember.FlameConfig.ResolutionMultiplier = 1.0;
            ember.FlameConfig.MotionDensity = 0.25;
            ember.FlameConfig.MutationChance = 0.33;
            ember.FlameConfig.AlternateSetpoint = 0.45;
            ember.FlameConfig.UnionSetpoint = 0.9;
            var flame = ember.Breed();
            flame.Wait();
            var rand = new Random((int)DateTime.Now.Ticks);
            var firstLine = formatFlameDesc(firstLines[rand.Next(firstLines.Count)], flame.Result);
            var secondLine = formatFlameDesc(secondLines[rand.Next(secondLines.Count)], flame.Result);
            var thirdLine = formatFlameDesc(thirdLines[rand.Next(thirdLines.Count)], flame.Result);
            var status = firstLine + "\n" + secondLine + "\n" + thirdLine + "\n" + formatFlameDesc(hashtags, flame.Result).Replace("-", "_");
            var tweet = twitterService.UploadSingleImageAsync(status, flame.Result.ImagePath);
            tweet.Wait();
            var animated = ember.Animate(flame.Result);
            animated.Wait();
            var animationLine = formatFlameDesc(animationLines[rand.Next(animationLines.Count)], flame.Result);

            twitterService.UploadVideoAsync(tweet.Result, animationLine + "\n" + formatFlameDesc(hashtags, flame.Result).Replace("-", "_"), animated.Result.VideoPath).Wait();
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
