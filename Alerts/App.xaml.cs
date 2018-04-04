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
        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static string candleStickWidthToString(CandleWidth width)
        {
            switch (width)
            {
                case CandleWidth.m1:
                    return "1m";
                case CandleWidth.m5:
                    return "5m";
                case CandleWidth.m15:
                    return "15m";
                case CandleWidth.m30:
                    return "30m";
                case CandleWidth.h1:
                    return "1h";
                case CandleWidth.h2:
                    return "2h";
                case CandleWidth.h4:
                    return "4h";
                case CandleWidth.h12:
                    return "12h";
                case CandleWidth.d1:
                    return "1d";
                case CandleWidth.w1:
                    return "1w";
                default:
                    return "init";
            }
        }

        public static CandleWidth stringToCandleStickWidth(string width)
        {
            switch (width)
            {
                case "1m":
                    return CandleWidth.m1;
                case "5m":
                    return CandleWidth.m5;
                case "15m":
                    return CandleWidth.m15;
                case "30m":
                    return CandleWidth.m30;
                case "1h":
                    return CandleWidth.h1;
                case "2h":
                    return CandleWidth.h2;
                case "4h":
                    return CandleWidth.h4;
                case "12h":
                    return CandleWidth.h12;
                case "1d":
                    return CandleWidth.d1;
                case "1w":
                    return CandleWidth.w1;
                default:
                    return CandleWidth.INIT;
            }
        }
    }
}
