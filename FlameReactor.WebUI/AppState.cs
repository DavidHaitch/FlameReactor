using FlameReactor.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlameReactor.WebUI
{
    public class AppState
    {
        public bool IsBreeding;
        public int PercentDone;
        public string ProgressMessage;
        public string AlertMessage
        {
            get { return _alertMessage; }
            set
            {
                _alertMessage = value;
                AlertMessageClass = AlertMessageClass == "messagefade1" ? "messagefade2" : "messagefade1";
            }
        }
        private string _alertMessage;
        public string AlertMessageClass = "messagefade1";
        public bool AlertEmpty { get { return string.IsNullOrWhiteSpace(AlertMessage); } }
        public bool IsIdle { get { return !IsBreeding; } }
        public Flame CurrentFlame
        {
            get
            {
                if (_currentFlame == null) _currentFlame = new EmberService("wwwroot/Flames/Pool").GetRandomFlames(1).First();
                //if (_currentFlame == null) _currentFlame = new EmberService("wwwroot/Flames/Pool").GetMostRecentFlame();
                return _currentFlame;

            }
            set
            {
                _currentFlame = value;
            }
        }

        private Flame _currentFlame;
    }
}
