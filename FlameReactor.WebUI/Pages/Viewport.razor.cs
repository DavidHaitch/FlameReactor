using Blazored.Modal;
using Blazored.Modal.Services;
using FlameReactor.WebUI.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using FlameReactor.WebUI.Shared;
using System.IO;
using FlameReactor.DB;
using FlameReactor.DB.Models;

namespace FlameReactor.WebUI.Pages
{
    public class ViewportModel : ComponentBase, IDisposable
    {
        [Inject]
        IJSRuntime JS { get; set; }

        [Inject]
        EmberService Ember { get; set; }

        [Inject]
        AppState AppState { get; set; }

        [Parameter]
        public int StateVar { get; set; }
        [CascadingParameter(Name = "IPAddress")] public string IPAddress { get; set; }

        [CascadingParameter] public IModalService Modal { get; set; }

        public ElementReference videobox;
        private Timer timer;

        protected bool IsFullscreen = false;
        protected bool voted = true;
        protected bool IsNotFullscreen => !IsFullscreen;
        protected string voteStyle => voted ? "color: gray" : "color: white";
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                var votename = IPAddress + "\t" + AppState.CurrentFlame.Name;
                using (var db = new FlameReactorContext())
                {
                    voted = db.Votes.Any(v => v.IPAddress == IPAddress && v.FlameId == AppState.CurrentFlame.ID);
                }

                Ember.RenderStep += Ember_RenderStep;
                Ember.RenderComplete += (sender, args) =>
                {
                    AppState.PercentDone = 0;
                    AppState.ProgressMessage = "";
                    InvokeAsync(() => StateHasChanged());
                    JS.InvokeVoidAsync("FR.playSound");
                    voted = false;
                };

                timer = new Timer();
                timer.Interval = 1000;
                timer.Elapsed += OnTimerInterval;
                timer.AutoReset = true;
                // Start the timer
                timer.Enabled = true;
                SetVideo("");

            }
            base.OnAfterRender(firstRender);
        }

        private async void Ember_RenderStep(object sender, RenderStepEventArgs args)
        {
            try
            {
                AppState.PercentDone = args.PercentDone;
                AppState.ProgressMessage = args.StepData;
                await InvokeAsync(() => StateHasChanged());
                await JS.InvokeVoidAsync("FR.logRenderStep", args);
            }
            catch
            { }
        }

        private void OnTimerInterval(object sender, ElapsedEventArgs e)
        {
            InvokeAsync(() => StateHasChanged());
        }

        private async void SetVideo(string path)
        {
            await JS.InvokeVoidAsync("FR.load", videobox);
        }

        protected async void OpenTwitter()
        {
            using (var db = new FlameReactorContext())
            {
                db.InteractionEvents.Add(new InteractionEvent()
                {
                    IPAddress = IPAddress,
                    Timestamp = DateTimeOffset.Now,
                    InteractionType = "TwitterClick",
                    Details = ""
                });
                db.SaveChanges();
            }
            await JS.InvokeVoidAsync("FR.OpenInNewTab", "https://twitter.com/FlameReactorBot");
        }

        protected void ShowAbout()
        {
            var options = new ModalOptions { HideCloseButton = false, Animation = ModalAnimation.FadeInOut(1), ContentScrollable = true };
            using (var db = new FlameReactorContext())
            {
                db.InteractionEvents.Add(new InteractionEvent()
                {
                    IPAddress = IPAddress,
                    Timestamp = DateTimeOffset.Now,
                    InteractionType = "ShowAbout",
                    Details = ""
                });
                db.SaveChanges();
            }

            Modal.Show<About>("What Is The Flame Reactor?", options);
        }

        protected async Task EnterFullscreen()
        {
            using (var db = new FlameReactorContext())
            {
                db.InteractionEvents.Add(new InteractionEvent()
                {
                    IPAddress = IPAddress,
                    Timestamp = DateTimeOffset.Now,
                    InteractionType = "EnterFullscreen",
                    Details = ""
                });
                db.SaveChanges();
            }
            IsFullscreen = true;
            await JS.InvokeVoidAsync("FR.ToggleFullscreen");
        }

        protected async Task ExitFullscreen()
        {
            IsFullscreen = false;
            using (var db = new FlameReactorContext())
            {
                db.InteractionEvents.Add(new InteractionEvent()
                {
                    IPAddress = IPAddress,
                    Timestamp = DateTimeOffset.Now,
                    InteractionType = "ExitFullscreen",
                    Details = ""
                });
                db.SaveChanges();
            }
            await JS.InvokeVoidAsync("FR.ToggleFullscreen");
        }

        protected void Upvote()
        {
            voted = true;
            using (var db = new FlameReactorContext())
            {
                db.InteractionEvents.Add(new InteractionEvent()
                {
                    IPAddress = IPAddress,
                    Timestamp = DateTimeOffset.Now,
                    InteractionType = "VoteManual",
                    Details = "Up"
                });
                db.SaveChanges();
            }
            Ember.Vote(IPAddress, AppState.CurrentFlame, 5);
        }

        protected void Downvote()
        {
            voted = true;
            using (var db = new FlameReactorContext())
            {
                db.InteractionEvents.Add(new InteractionEvent()
                {
                    IPAddress = IPAddress,
                    Timestamp = DateTimeOffset.Now,
                    InteractionType = "VoteManual",
                    Details = "Down"
                });
                db.SaveChanges();
            }
            Ember.Vote(IPAddress, AppState.CurrentFlame, -5);
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
