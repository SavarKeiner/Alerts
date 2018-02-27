using Alerts.Logic.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Alerts
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static string candleStickWidthToString(CandleStickWidth width)
        {
            switch (width)
            {
                case CandleStickWidth.m5:
                    return "5m";
                case CandleStickWidth.m15:
                    return "15m";
                case CandleStickWidth.m30:
                    return "30m";
                case CandleStickWidth.h1:
                    return "1h";
                case CandleStickWidth.h2:
                    return "2h";
                case CandleStickWidth.h4:
                    return "4h";
                case CandleStickWidth.h12:
                    return "12h";
                case CandleStickWidth.d1:
                    return "1d";
                case CandleStickWidth.w1:
                    return "1w";
                default:
                    return "init";
            }
        }

        public static CandleStickWidth stringToCandleStickWidth(string width)
        {
            switch (width)
            {
                case "5m":
                    return CandleStickWidth.m5;
                case "15m":
                    return CandleStickWidth.m15;
                case "30m":
                    return CandleStickWidth.m30;
                case "1h":
                    return CandleStickWidth.h1;
                case "2h":
                    return CandleStickWidth.h2;
                case "4h":
                    return CandleStickWidth.h4;
                case "12h":
                    return CandleStickWidth.h12;
                case "1d":
                    return CandleStickWidth.d1;
                case "1w":
                    return CandleStickWidth.w1;
                default:
                    return CandleStickWidth.INIT;
            }
        }
    }
}
