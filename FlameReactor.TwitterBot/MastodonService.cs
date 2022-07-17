using FlameReactor.DB.Models;
using Mastonet;
using Mastonet.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor.TwitterBot
{
    public class MastodonService
    {
        private readonly MastodonClient _client;
        public MastodonService(IConfiguration configuration)
        {
            var loginTask = Login(configuration);
            loginTask.Wait();
            _client = loginTask.Result;
        }

        public async Task<Status> UploadSingleImageAsync(string status, string imageFile)
        {
            var m = await _client.UploadMedia(new MediaDefinition(File.OpenRead(imageFile), imageFile), "Fractal imagery.");
            var s = await _client.PostStatus(status, mediaIds: new[] { m.Id });
            return s;
        }

        public async Task<Status> UploadVideoAsync(Status toot, string status, string videoFile)
        {
            var m = await _client.UploadMedia(new MediaDefinition(File.OpenRead(videoFile), videoFile), "Fractal imagery.");
            var s = await _client.PostStatus(status, replyStatusId: toot.Id, mediaIds: new[] { m.Id });
            return s;
        }

        public async Task<TweetRecord> GetCountsForTootAsync(TweetRecord tweet)
        {
            try
            {
                var s = await _client.GetStatus((long)tweet.ID);
                tweet.Faves = s.FavouritesCount;
                tweet.Retweets = s.ReblogCount;
            }
            catch(Exception ex)
            {
                File.AppendAllText("error.log", "--------------" + "\n");
                File.AppendAllText("error.log", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\n");
                File.AppendAllText("error.log", ex.Message + "\n");
                File.AppendAllText("error.log", "--------------" + "\n");
            }
            return tweet;
        }

        private async Task<MastodonClient> Login(IConfiguration configuration)
        {
            var appRegistration = new AppRegistration
            {
                Instance = "mastodon.social",
                ClientId = configuration["Mastodon:ClientId"],
                ClientSecret = configuration["Mastodon:ClientSecret"],
                RedirectUri = "urn:ietf:wg:oauth:2.0:oob",
                Scope = Scope.Read | Scope.Write | Scope.Follow,
                Id = Convert.ToInt64(configuration["Mastodon:Id"])
            };
            var authClient = new AuthenticationClient(appRegistration);
            var uri = authClient.OAuthUrl();
            var auth = new Auth
            {
                AccessToken = configuration["Mastodon:AccessToken"]
            };
            return new MastodonClient(appRegistration, auth);
        }
    }
}
