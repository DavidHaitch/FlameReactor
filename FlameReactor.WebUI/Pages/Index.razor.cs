using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace FlameReactor.WebUI.Pages
{
    public partial class IndexModel : ComponentBase
    {
        [Inject]
        IJSRuntime JS { get; set; }

        public int StateVar;
        public void UpdateState(int s)
        {
            StateVar = s;
        }

        protected async Task ShowUI()
        {
            await JS.InvokeVoidAsync("FR.WakeUI");
        }
    }
}