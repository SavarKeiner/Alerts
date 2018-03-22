using Alerts.Logic.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alerts.Logic.Events
{
    public class CandlePulledEventArgs : EventArgs
    {
        public JArray klinesArray { get; set; }
        public long[] OpenTime { get; set; }
        public double[] Open { get; set; }
        public double[] High { get; set; }
        public double[] Low { get; set; }
        public double[] close { get; set; }
        public double[] Volume { get; set; }
        public long[] CloseTime { get; set; }
        public double[] QuoteAssetVolume { get; set; }
        public long[] NumberOfTrades { get; set; }
        public double[] TakerBuyBaseAssetVolume { get; set; }
        public double[] TakerBuyQuoteAssetVolume { get; set; }
        public int Lenght { get; set; }
        public CandleWidth Width { get; set; }
}
}
/*
 [
  [
    1499040000000,      // Open time
    "0.01634790",       // Open
    "0.80000000",       // High
    "0.01575800",       // Low
    "0.01577100",       // Close
    "148976.11427815",  // Volume
    1499644799999,      // Close time
    "2434.19055334",    // Quote asset volume
    308,                // Number of trades
    "1756.87402397",    // Taker buy base asset volume
    "28.46694368",      // Taker buy quote asset volume
    "17928899.62484339" // Ignore
  ]
]
*/
