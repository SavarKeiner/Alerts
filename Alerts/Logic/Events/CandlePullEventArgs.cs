using Alerts.Logic.Enums;
using Alerts.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alerts.Logic.Events
{
    public class CandlePullEventArgs : EventArgs
    {
        public List<CandleIF> candleList;
        public CandleWidth Width { get; set; }
    }
}
