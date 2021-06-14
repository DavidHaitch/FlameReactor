using FlameReactor.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor.FlameActions
{
    public abstract class FlameAction
    {
        protected Action<RenderStepEventArgs> StepEvent;
        public FlameAction(Action<RenderStepEventArgs> stepEvent)
        {
            StepEvent = stepEvent;
        }

        public abstract Task<Flame[]> Act(FlameConfig flameConfig, params Flame[] flames);

    }
}
