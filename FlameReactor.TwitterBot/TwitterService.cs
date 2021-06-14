
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

        public async Task UploadVideoAsync(Status tweet, string status, string videoFile)
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
                return;
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
            }
            else
            {
                MediaError? error = mediaStatusResponse?.ProcessingInfo?.Error;

                if (error != null)
                    Console.WriteLine($"Request failed - Code: {error.Code}, Name: {error.Name}, Message: {error.Message}");
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
                return null;
            }

            Status? tweet = await twitterCtx.TweetAsync(status, new ulong[] { media.MediaID });
            if (tweet != null) return tweet;
            return null;
        }

    }
}
