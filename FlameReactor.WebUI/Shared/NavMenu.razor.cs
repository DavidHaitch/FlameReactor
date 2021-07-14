using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using FlameReactor.WebUI.Shared;
using FlameReactor.DB.Models;
using FlameReactor.DB;

namespace FlameReactor.WebUI.Shared
{
    public class NavMenuModel : ComponentBase, IDisposable
    {
        [CascadingParameter] public IModalService Modal { get; set; }

        [Inject]
        IJSRuntime JS { get; set; }

        [Inject]
        AppState AppState { get; set; }

        [Inject]
        EmberService Ember { get; set; }
        [Parameter]
        public EventCallback<int> OnStateChange { get; set; }
        private bool collapseNavMenu = true;
        protected List<Flame> FlameChoices = new List<Flame>();
        public ElementReference alertbell;

        private Timer timer;
        [CascadingParameter(Name = "IPAddress")] public string IPAddress { get; set; }
        [CascadingParameter(Name = "UserAgent")] public string UserAgent { get; set; }
        protected string NavMenuCssClass => collapseNavMenu ? "collapse" : null;
        private void OnTimerInterval(object sender, ElapsedEventArgs e)
        {
            InvokeAsync(() => StateHasChanged());
        }
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                if (!string.IsNullOrWhiteSpace(IPAddress))
                {
                    using (var db = new FlameReactorContext())
                    {
                        db.AccessEvents.Add(new AccessEvent()
                        {
                            IPAddress = IPAddress,
                            Timestamp = DateTimeOffset.Now,
                            UserAgent = UserAgent
                        });
                        db.SaveChanges();
                    }
                }

                FlameChoices = Ember.GetEligibleFlames(4, AppState.CurrentFlame);
                timer = new Timer();
                timer.Interval = 1000;
                timer.Elapsed += OnTimerInterval;
                timer.AutoReset = true;
                // Start the timer
                timer.Enabled = true;
            }

            base.OnAfterRender(firstRender);
        }

        protected void ChooseFlame(Flame f)
        {
            JS.InvokeVoidAsync("FR.playSound");
            FlameChoices.ForEach(fc =>
            {
                if(fc.Name != f.Name)
                {
                    //using(var db = new FlameReactorContext())
                    //{
                    //    db.InteractionEvents.Add(new InteractionEvent()
                    //    {
                    //        IPAddress = IPAddress,
                    //        Timestamp = DateTimeOffset.Now,
                    //        InteractionType = "Nonselect-Downvote",
                    //        Details = "Nonselection-downvoting " + fc.Name
                    //    });
                    //    db.SaveChanges();
                    //}

                    //Ember.Vote(IPAddress, fc, -1, true);
                }
                else
                {
                    using (var db = new FlameReactorContext())
                    {
                        db.InteractionEvents.Add(new InteractionEvent()
                        {
                            IPAddress = IPAddress,
                            Timestamp = DateTimeOffset.Now,
                            InteractionType = "Select-Upvote",
                            Details = "Selection-upvoting " + fc.Name
                        });
                        db.SaveChanges();
                    }
                    Ember.Vote(IPAddress, fc, 2, true);
                }
            });
            if (AppState.IsIdle)
            {
                using (var db = new FlameReactorContext())
                {
                    db.InteractionEvents.Add(new InteractionEvent()
                    {
                        IPAddress = IPAddress,
                        Timestamp = DateTimeOffset.Now,
                        InteractionType = "Breed",
                        Details = "Breeding " + AppState.CurrentFlame.Name + " with " + f.Name
                    });
                    db.SaveChanges();
                }
                DoChooseFlame(f);
            }
        }

        protected async Task NameFlame(Flame f)
        {
            var param = new ModalParameters();
            param.Add("flame", f);

            var options = new ModalOptions { HideCloseButton = true,Animation = ModalAnimation.FadeInOut(1) };
            var m = Modal.Show<NameFlameModal>("Name this flame", param, options);
            var name = await m.Result;
            if (!name.Cancelled && !string.IsNullOrWhiteSpace(name.Data.ToString()) && name.Data.ToString().Length < 40)
            {
                f.DisplayName = name.Data.ToString();
                Ember.ChangeFlame(f);
                using (var db = new FlameReactorContext())
                {
                    db.InteractionEvents.Add(new InteractionEvent()
                    {
                        IPAddress = IPAddress,
                        Timestamp = DateTimeOffset.Now,
                        InteractionType = "Rename",
                        Details = "Renaming " + f.Name + " to " + f.DisplayName
                    });
                    db.SaveChanges();
                }
            }
        }

        async Task DoChooseFlame(Flame f)
        {
            try
            {
                AppState.IsBreeding = true;
                InvokeAsync(() => StateHasChanged());
                await OnStateChange.InvokeAsync(1);
                var tempFlame = await Task.Run(() => Ember.Breed(f, AppState.CurrentFlame));
                AppState.CurrentFlame = await Task.Run(() => Ember.Animate(tempFlame));
                AppState.AlertMessage = AppState.CurrentFlame.DisplayName;
                await OnStateChange.InvokeAsync(0);
                if (File.Exists("./wwwroot/current.png")) File.Delete("./wwwroot/current.png");
                File.Copy(AppState.CurrentFlame.ImagePath, "./wwwroot/current.png");
                FlameChoices = Ember.GetEligibleFlames(4, AppState.CurrentFlame);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                AppState.IsBreeding = false;
                InvokeAsync(() => StateHasChanged());
                await OnStateChange.InvokeAsync(0);
            }
        }

        public void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
