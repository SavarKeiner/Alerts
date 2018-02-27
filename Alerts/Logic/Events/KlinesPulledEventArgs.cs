using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alerts.Logic.Events
{
    public class KlinesPulledEventArgs : EventArgs
    {
        public JArray klinesArray { get; set; }
        public JToken Change { get; set; }
        public double[] close { get; set; }
        public int Lenght { get; set; }
        public long lastTime { get; set; }
    }
}
