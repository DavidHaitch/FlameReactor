
using FlameReactor.DB;
using System.Linq;
using LinqToTwitter;
using LinqToTwitter.OAuth;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using LinqToTwitter.Common;
using FlameReactor.DB.Models;

namespace FlameReactor.TwitterBot
{
    public class TwitterService
    {
        private IAuthorizer auth;

        public TwitterService(IConfiguration configuration)
        {
            auth = new SingleUserAuthorizer()
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = configuration["Twitter:ConsumerKey"],
                    ConsumerSecret = configuration["Twitter:ConsumerSecret"],
                    AccessToken = configuration["Twitter:AccessToken"],
                    AccessTokenSecret = configuration["Twitter:AccessTokenSecret"]
                }
            };

            auth.AuthorizeAsync().Wait();
        }

        public async Task<Status?> UploadVideoAsync(Status tweet, string status, string videoFile)
        {
            var twitterCtx = new TwitterContext(auth);
            byte[] imageBytes = File.ReadAllBytes(videoFile);
            var additionalOwners = new ulong[] { };
            string mediaType = "video/mp4";
            string mediaCategory = "tweet_video";

            Media? media = await twitterCtx.UploadMediaAsync(imageBytes, mediaType, additionalOwners, mediaCategory);

            if (media == null)
            {
                Console.WriteLine("Invalid Media returned from UploadMediaAsync");
                File.AppendAllText("./error.log", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " Invalid Media returned from UploadMediaAsync" + "\n");
                return null;
            }

            Media? mediaStatusResponse = null;
            do
            {
                if (mediaStatusResponse != null)
                {
                    int checkAfterSeconds = mediaStatusResponse?.ProcessingInfo?.CheckAfterSeconds ?? 0;
                    await Task.Delay(checkAfterSeconds * 1000);
                }

                mediaStatusResponse =
                    await
                    (from stat in twitterCtx.Media
                     where stat.Type == MediaType.Status &&
                           stat.MediaID == media.MediaID
                     select stat)
                    .SingleOrDefaultAsync();
            } while (mediaStatusResponse?.ProcessingInfo?.State == MediaProcessingInfo.InProgress);

            if (mediaStatusResponse?.ProcessingInfo?.State == MediaProcessingInfo.Succeeded)
            {
                Status? reply = await twitterCtx.ReplyAsync(tweet.StatusID, status, new ulong[] { media.MediaID });

                if (reply != null)
                    Console.WriteLine($"Tweet sent: {reply.Text}");

                return reply;
            }
            else
            {
                MediaError? error = mediaStatusResponse?.ProcessingInfo?.Error;

                if (error != null)
                {
                    Console.WriteLine($"Request failed - Code: {error.Code}, Name: {error.Name}, Message: {error.Message}");
                    File.AppendAllText("./error.log", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + $" Video Request failed - Code: {error.Code}, Name: {error.Name}, Message: {error.Message}" + "\n");
                }

                return null;
            }
        }

        public async Task<Status> UploadSingleImageAsync(string status, string imageFile)
        {
            string mediaCategory = "tweet_image";
            var twitterCtx = new TwitterContext(auth);
            byte[] imageBytes = File.ReadAllBytes(imageFile);
            var additionalOwners = new ulong[] { };
            string mediaType = "image/png";

            Media? media = await twitterCtx.UploadMediaAsync(imageBytes, mediaType, additionalOwners, mediaCategory);
            if (media == null)
            {
                Console.WriteLine("Invalid Media returned from UploadMediaAsync");
                File.AppendAllText("./error.log", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " Invalid Media returned from UploadMediaAsync" + "\n");
                return null;
            }

            await Task.Delay(1000);
            try
            {
                Status? tweet = await twitterCtx.TweetAsync(status, new ulong[] { media.MediaID });
                if (tweet != null) return tweet;
            }
            catch (TwitterQueryException ex)
            {
                File.AppendAllText("error.log", "--------------" + "\n");
                File.AppendAllText("error.log", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\n");
                File.AppendAllText("error.log", ex.Message + "\n");
                foreach (var error in ex.Errors)
                {
                    File.AppendAllText("error.log", "\t" + error.Code + "|" + error.Message + "|" + error.Request + "\n");
                }
                File.AppendAllText("error.log", ex.Type + "\n");
                File.AppendAllText("error.log", ex.Details + "\n");
                File.AppendAllText("error.log", ex.StackTrace + "\n");
                File.AppendAllText("error.log", "--------------" + "\n");
            }

            return null;
        }

        public async Task<int> GetStatusRequestsRemaining()
        {
            try
            {
                var twitterCtx = new TwitterContext(auth);
                var helpResponse = await
                    (from help in twitterCtx.Help
                     where help.Type == HelpType.RateLimits
                     select help)
                    .SingleOrDefaultAsync();
                return helpResponse.RateLimits["statuses"].First(rl => rl.Resource == "/statuses/user_timeline").Remaining;
            }
            catch
            {
                return 0;
            }

        }

        public async Task<TweetRecord> GetCountsForTweetAsync(TweetRecord tweet)
        {
            var twitterCtx = new TwitterContext(auth);
            var status = await
                (from s in twitterCtx.Status
                 where s.StatusID == tweet.ID
                 && s.Type == StatusType.User
                 select s).ToListAsync();
            if (status != null && status.Any())
            {
                tweet.Faves = status.First().FavoriteCount.GetValueOrDefault();
                tweet.Retweets = status.First().RetweetCount;
            }

            return tweet;
        }

        public async Task<ulong> GetTweetIdForFlameName(string flameName)
        {
            var twitterCtx = new TwitterContext(auth);
            Search? searchResponse =
                           await
                           (from search in twitterCtx.Search
                            where search.Type == SearchType.Search &&
                                  search.Query == flameName &&
                                  search.IncludeEntities == true &&
                                  search.TweetMode == TweetMode.Extended
                            select search)
                           .SingleOrDefaultAsync();
            if (searchResponse != null && searchResponse.Statuses.Any())
            {
                return searchResponse.Statuses.OrderBy(s => s.CreatedAt).First().StatusID;
            }
            else
            {
                return 0;
            }

        }
    }
}
