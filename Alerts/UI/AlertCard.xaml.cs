using Alerts.Logic.Enums;
using Alerts.Logic.Events;
using Alerts.Logic.Interfaces;
using Alerts.Logic.RESTObjects;
using Alerts.UI.Graphs;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Alerts.UI
{
    /// <summary>
    /// Interaction logic for AlertCard.xaml
    /// </summary>
    public partial class AlertCard : UserControl
    {
        private CandleWidth _candleWidth;
        private int maxPoints = 20;
        public CandleWidth CandleWidth
        {
            get
            {
                return _candleWidth;
            }
            set
            {
                _candleWidth = value;
                klineWidth.Content = App.candleStickWidthToString(value);

                var step = value.ToString()[0];

                if (value != CandleWidth.INIT)
                {
                    int mul = 4;

                    if (step == 'm')
                    {

                        AxisStep = (long)TimeSpan.FromMinutes(int.Parse(value.ToString().Substring(1))).TotalMilliseconds * mul;
                        AxisUnit = TimeSpan.FromMinutes(int.Parse(value.ToString().Substring(1))).TotalMilliseconds * mul;
                    }
                    else if (step == 'h')
                    {
                        AxisStep = (long)TimeSpan.FromHours(int.Parse(value.ToString().Substring(1))).TotalMilliseconds * mul;
                        AxisUnit = TimeSpan.FromHours(int.Parse(value.ToString().Substring(1))).TotalMilliseconds * mul;
                    }
                    else if (step == 'd')
                    {
                        AxisStep = (long)TimeSpan.FromDays(int.Parse(value.ToString().Substring(1))).TotalMilliseconds * mul;
                        AxisUnit = TimeSpan.FromDays(int.Parse(value.ToString().Substring(1))).TotalMilliseconds * mul;
                        FormatterX = value1 => (new DateTime(1970, 1, 1)).AddMilliseconds((long)value1).ToString("MM:dd");
                    }
                }
            }
        }
        private Indicators _Indicator;
        public Indicators Indicator
        {
            get
            {
                return _Indicator;
            }
            set
            {
                if (value == Indicators.INIT)
                    return;

                _Indicator = value;
                indicatorName.Content = value;

                SetGraph(value);

            }
        }
        public Exchanges Exchange { get; set; }
        public Coins Coin { get; set; }
        public Coins _Pair;
        public Coins Pair
        {
            get
            {
                return _Pair;
            }
            set
            {
                if (value == Coins.INIT)
                    return;

                _Pair = value;
            }
        }
        private double _Value;
        public double Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                indicatorValue.Content = value.ToString("0.00000000", CultureInfo.InvariantCulture);
            }
        }
        private double _ValueChange;
        private double ValueChange
        {
            get
            {
                return _ValueChange;
            }
            set
            {
                _ValueChange = value;
                indicatorChange.Content = value.ToString("0.00000000", CultureInfo.InvariantCulture);
            }

        }
        private AlertLayout alert { get; set; }

        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> FormatterX { get; set; }
        public Func<double, string> FormatterY { get; set; }
        public Func<ChartPoint, string> LabelPoint { get; set; }
        public long AxisStep { get; set; }
        public double AxisUnit { get; set; }

        public AlertCard(AlertLayout alert, CandleWidth CandleWidth, Exchanges Exchange, Coins Coin, Coins Pair, Indicators Indicator)
        {
            InitializeComponent();

            this.Indicator = Indicator; //needs indicator bevor candlewidth
            this.CandleWidth = CandleWidth;
            this.Exchange = Exchange;
            this.Coin = Coin;
            this.Pair = Pair;
            this.alert = alert;

            btnRemoveCard.Click += (o, e) =>
            {
                alert.removeTo(this);
            };

            //15212094872460000000
            FormatterX = value => (new DateTime(1970, 1, 1)).AddMilliseconds((long)value).ToString("HH:mm");
            FormatterY = value => (value.ToString("0.00######", CultureInfo.InvariantCulture));
            //LabelPoint = chartPoint => chartPoint.Y.ToString("0.00") + (new DateTime(1970, 1, 1)).AddMilliseconds(chartPoint.X).ToString("yyyy:MM:dd:HH:mm:ss");
            //Formatter = value => (new DateTime(1970, 1, 1)).AddTicks((long)value).ToString("HH:mm");
            DataContext = this;
        }

        private void calcConditions(double oldValue, double newValue, long openTime)
        {
            Application curApp = Application.Current;
            MainWindow mainWindow = (MainWindow)curApp.MainWindow;

            foreach (CellCondition condition in listCondition.Children)
            {
                switch (condition.indicatorCondition)
                {
                    case IndicatorConditions.ABOVE:
                        if (oldValue <= condition.value && newValue >= condition.value)
                        {
                            if (condition.lastSeenOpenTime < openTime)
                            {
                                condition.lastSeenOpenTime = openTime;
                                condition.showNotification(Coin.ToString() + Pair.ToString() + " " + Exchange + " " + Indicator + " " + App.candleStickWidthToString(CandleWidth) + " " + condition.indicatorCondition + " " + condition.value.ToString("0.00########"));
                            }
                        }
                        break;
                    case IndicatorConditions.BELOW:
                        if (oldValue >= condition.value && newValue <= condition.value)
                        {
                            if (condition.lastSeenOpenTime < openTime)
                            {
                                condition.lastSeenOpenTime = openTime;
                                condition.showNotification(Coin.ToString() + Pair.ToString() + " " + Exchange + " " + Indicator + " " + App.candleStickWidthToString(CandleWidth) + " " + condition.indicatorCondition + " " + condition.value.ToString("0.00########"));
                            }
                        }
                        break;
                    case IndicatorConditions.CROSS:
                        if (oldValue >= condition.value && newValue <= condition.value)
                        {
                            if (condition.lastSeenOpenTime < openTime)
                            {
                                condition.lastSeenOpenTime = openTime;
                                condition.showNotification(Coin.ToString() + Pair.ToString() + " " + Exchange + " " + Indicator + " " + App.candleStickWidthToString(CandleWidth) + " " + condition.indicatorCondition + " " + condition.value.ToString("0.00########"));
                            }

                        }
                        else if (oldValue <= condition.value && newValue >= condition.value)
                        {
                            if (condition.lastSeenOpenTime < openTime)
                            {
                                condition.lastSeenOpenTime = openTime;
                                condition.showNotification(Coin.ToString() + Pair.ToString() + " " + Exchange + " " + Indicator + " " + App.candleStickWidthToString(CandleWidth) + " " + condition.indicatorCondition + " " + condition.value.ToString("0.00########"));
                            }
                        }
                        break;
                    case IndicatorConditions.CHANGE:
                        if (Math.Abs(newValue - oldValue) >= condition.value)
                        {
                            if (condition.lastSeenOpenTime < openTime)
                            {
                                condition.lastSeenOpenTime = openTime;
                                condition.showNotification(Coin.ToString() + Pair.ToString() + " " + Exchange + " " + Indicator + " " + condition.indicatorCondition + " " + condition.value.ToString("0.00########"));
                            }
                        }
                        break;
                }
            }
        }

        private void calculateAroonOsc(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.AroonOsc(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateBop(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.Bop(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdl2Crows(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.Cdl2Crows(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdl3BlackCrows(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.Cdl3BlackCrows(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdl3Inside(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.Cdl3Inside(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdl3LineStrike(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.Cdl3LineStrike(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdl3Outside(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.Cdl3Outside(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdl3StarsInSouth(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.Cdl3StarsInSouth(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdl3WhiteSoldiers(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.Cdl3WhiteSoldiers(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlAdvanceBlock(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlAdvanceBlock(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlBeltHold(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlBeltHold(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlBreakaway(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlBreakaway(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlClosingMarubozu(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlClosingMarubozu(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlConcealBabysWall(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlConcealBabysWall(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlCounterAttack(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlCounterAttack(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlDoji(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlDoji(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlDojiStar(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlDojiStar(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlDragonflyDoji(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlDragonflyDoji(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlEngulfing(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlEngulfing(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlGapSideSideWhite(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlGapSideSideWhite(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlGravestoneDoji(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlGravestoneDoji(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlHammer(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlHammer(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlHangingMan(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlHangingMan(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlHarami(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlHarami(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlHaramiCross(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlHaramiCross(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlXSideGap3Methods(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlXSideGap3Methods(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlUpsideGap2Crows(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlUpsideGap2Crows(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlUnique3River(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlUnique3River(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlTristar(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlTristar(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlThrusting(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlThrusting(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlTasukiGap(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlTasukiGap(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlTakuri(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlTakuri(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlStickSandwhich(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlStickSandwhich(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlStalledPattern(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlStalledPattern(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlSpinningTop(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlSpinningTop(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlShortLine(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlShortLine(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlShootingStar(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlShootingStar(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlSeperatingLines(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlSeperatingLines(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlRiseFall3Methods(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlRiseFall3Methods(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlRickshawMan(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlRickshawMan(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlPiercing(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlPiercing(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlOnNeck(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlOnNeck(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlMatchingLow(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlMatchingLow(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlMarubozu(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlMarubozu(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlLongLine(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlLongLine(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlLongLeggedDoji(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlLongLeggedDoji(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlLadderBottom(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlLadderBottom(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlKickingByLength(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlKickingByLength(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlKicking(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlKicking(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlInvertedHammer(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlInvertedHammer(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlInNeck(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlInNeck(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlIdentical3Crows(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlIdentical3Crows(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlHomingPigeon(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlHomingPigeon(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlHikkakeMod(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlHikkakeMod(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlHikkake(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlHikkake(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCdlHignWave(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            int[] c = new int[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.CdlHignWave(0, list.Count - 1, list.Select(x => x.getOpen()).ToArray(), list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }


        private void calculateCeil(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Ceil(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateExp(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Exp(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateHtDcPeriod(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.HtDcPeriod(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateHtDcPhase(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.HtDcPhase(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateHtTrendline(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.HtTrendline(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateHtTrendMode(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            int[] c = new int[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.HtTrendMode(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateLn(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Ln(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateLog10(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Log10(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateMfi(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int optInTimePeriod = 14;
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Mfi(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), list.Select(x => x.getVolume()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculatePlusDM(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int optInTimePeriod = 14;
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.PlusDM(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateTypPrice(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.TypPrice(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateWclPrice(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.WclPrice(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateAdxr(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int optInTimePeriod = 14;
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Adxr(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateMinusDi(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int optInTimePeriod = 14;
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.MinusDI(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculatePlusDi(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int optInTimePeriod = 14;
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.PlusDI(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateWillR(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int optInTimePeriod = 14;
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.WillR(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateMom(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Mom(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCmo(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Cmo(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateDema(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Dema(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateEma(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Ema(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateKama(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Kama(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateLinearReg(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.LinearReg(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateLinearRegAngle(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.LinearRegAngle(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateLinearRegIntercept(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.LinearRegIntercept(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateLinearRegSlope(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Rsi(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateMax(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Max(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateRocP(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.RocP(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateRoc(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Roc(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateRocR100(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.RocR100(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateRocR(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.RocR(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateSma(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Sma(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateTema(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Tema(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateTrima(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Trima(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateTsf(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Tsf(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateWma(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Wma(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateTrange(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.TrueRange(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateNatr(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Natr(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];


            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateAtr(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Atr(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];


            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateDx(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int optInTimePeriod = 14;
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Dx(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateAdx(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int optInTimePeriod = 14;
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Adx(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateCci(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int optInTimePeriod = 14;
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Cci(0, list.Count - 1, list.Select(x => x.getHigh()).ToArray(), list.Select(x => x.getLow()).ToArray(), list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateObv(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            TicTacTec.TA.Library.Core.Obv(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), list.Select(x => x.getVolume()).ToArray(), out a, out b, c);


            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculateTrix(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 18;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Trix(0, e.candleList.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] * 100 });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = e.candleList[e.candleList.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1] * 100;
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] * 100 });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1] * 100;
            ValueChange = (c[b - 1] - c[b - 2]) * 100;

            calcConditions(c[b - 2] * 100, c[b - 1] * 100, list[list.Count - 1].getOpenTime());
        }

        private void calculateRsi(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }

            TicTacTec.TA.Library.Core.Rsi(0, list.Count - 1, list.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = c[b - pointsToDraw + i] });
                }
            }
            else if (model.Count == pointsToDraw) //charts is filled
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime) //same candle
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)//new candle
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b - 1];
            ValueChange = c[b - 1] - c[b - 2];

            calcConditions(c[b - 2], c[b - 1], list[list.Count - 1].getOpenTime());
        }

        private void calculatePrice(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (list.Count <= 1) //check if indicator can even be calculated
            {
                Value = 0.0d;
                ValueChange = 0.0d;
                return;
            }


            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = list[list.Count - pointsToDraw + i].getClose() });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = list[list.Count - 1].getClose();
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = list[list.Count - 1].getClose() });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = list[list.Count - 1].getClose() });
            }

            Value = list[list.Count - 1].getClose();
            ValueChange = list[list.Count - 1].getClose() - list[list.Count - 2].getClose();

            if (ValueChange >= 0)
                indicatorChange.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));
            else
                indicatorChange.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));

            calcConditions(list[list.Count - 2].getClose(), list[list.Count - 1].getClose(), list[list.Count - 1].getOpenTime());
        }

        private void calculateVolume(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int pointsToDraw = App.Clamp(list.Count, 0, maxPoints); //if there are less points to draw then set in maxPoints

            if (model.Count == 0 || e.newList == true) //first run or pulling skipped candles
            {
                model.Clear();
                for (int i = 0; i < pointsToDraw; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - pointsToDraw + i].getOpenTime(), Value = list[list.Count - pointsToDraw + i].getVolume() });
                }
            }
            else if (model.Count == pointsToDraw)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = list[list.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = list[list.Count - 1].getVolume();
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = last.getVolume() });

                }
            }
            else if (model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = last.getVolume() });
            }

            Value = list[list.Count - 1].getVolume();
            ValueChange = list[list.Count - 1].getVolume() - list[list.Count - 2].getVolume();

            if (ValueChange >= 0)
                indicatorChange.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));
            else
                indicatorChange.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));

            calcConditions(list[list.Count - 2].getClose(), list[list.Count - 1].getClose(), list[list.Count - 1].getOpenTime());
        }

        public void SetGraphValues(CandlePullEventArgs e)
        {
            try
            {
                if (Indicator == Indicators.RSI)
                    calculateRsi(e);
                else if (Indicator == Indicators.PRICE)
                    calculatePrice(e);
                else if (Indicator == Indicators.VOLUME)
                    calculateVolume(e);
                else if (Indicator == Indicators.TRIX)
                    calculateTrix(e);
                else if (Indicator == Indicators.OBV)
                    calculateObv(e);
                else if (Indicator == Indicators.CCI)
                    calculateCci(e);
                else if (Indicator == Indicators.ADX)
                    calculateAdx(e);
                else if (Indicator == Indicators.DX)
                    calculateDx(e);
                else if (Indicator == Indicators.ATR)
                    calculateAtr(e);
                else if (Indicator == Indicators.NATR)
                    calculateNatr(e);
                else if (Indicator == Indicators.TRUERANGE)
                    calculateTrange(e);
                else if (Indicator == Indicators.MOM)
                    calculateMom(e);
                else if (Indicator == Indicators.CMO)
                    calculateCmo(e);
                else if (Indicator == Indicators.DEMA)
                    calculateDema(e);
                else if (Indicator == Indicators.EMA)
                    calculateEma(e);
                else if (Indicator == Indicators.KAMA)
                    calculateKama(e);
                else if (Indicator == Indicators.LinReg)
                    calculateLinearReg(e);
                else if (Indicator == Indicators.LinRegAngle)
                    calculateLinearRegAngle(e);
                else if (Indicator == Indicators.LinRegSlope)
                    calculateLinearRegSlope(e);
                else if (Indicator == Indicators.LinRegIntercept)
                    calculateLinearRegIntercept(e);
                else if (Indicator == Indicators.MAX)
                    calculateMax(e);
                else if (Indicator == Indicators.ROCP)
                    calculateRocP(e);
                else if (Indicator == Indicators.ROCR)
                    calculateRocR(e);
                else if (Indicator == Indicators.SMA)
                    calculateSma(e);
                else if (Indicator == Indicators.TEMA)
                    calculateTema(e);
                else if (Indicator == Indicators.TRIMA)
                    calculateTrima(e);
                else if (Indicator == Indicators.TSF)
                    calculateTsf(e);
                else if (Indicator == Indicators.WMA)
                    calculateWma(e);
                else if (Indicator == Indicators.WillR)
                    calculateWillR(e);
                else if (Indicator == Indicators.PLUSDI)
                    calculatePlusDi(e);
                else if (Indicator == Indicators.MinusDI)
                    calculateMinusDi(e);
                else if (Indicator == Indicators.ADXR)
                    calculateAdxr(e);
                else if (Indicator == Indicators.WclPrice)
                    calculateWclPrice(e);
                else if (Indicator == Indicators.TypPrice)
                    calculateTypPrice(e);
                else if (Indicator == Indicators.RocR100)
                    calculateRocR100(e);
                else if (Indicator == Indicators.Roc)
                    calculateRoc(e);
                else if (Indicator == Indicators.PlusDM)
                    calculatePlusDM(e);
                else if (Indicator == Indicators.Log10)
                    calculateLog10(e);
                else if (Indicator == Indicators.Ln)
                    calculateLn(e);
                else if (Indicator == Indicators.HtTrendMode)
                    calculateHtTrendMode(e);
                else if (Indicator == Indicators.HtTrendline)
                    calculateHtTrendline(e);
                else if (Indicator == Indicators.HtDcPhase)
                    calculateHtDcPhase(e);
                else if (Indicator == Indicators.HtDcPeriod)
                    calculateHtDcPeriod(e);
                else if (Indicator == Indicators.Exp)
                    calculateExp(e);
                else if (Indicator == Indicators.Ceil)
                    calculateCeil(e);
                else if (Indicator == Indicators.Mfi)
                    calculateMfi(e);
                else if (Indicator == Indicators.AroonOsc)
                    calculateAroonOsc(e);
                else if (Indicator == Indicators.Bop)
                    calculateBop(e);
                else if (Indicator == Indicators.Cdl2Crows)
                    calculateCdl2Crows(e);
                else if (Indicator == Indicators.Cdl3BlackCrows)
                    calculateCdl3BlackCrows(e);
                else if (Indicator == Indicators.Cdl3Inside)
                    calculateCdl3Inside(e);
                else if (Indicator == Indicators.Cdl3LineStrike)
                    calculateCdl3LineStrike(e);
                else if (Indicator == Indicators.Cdl3Outside)
                    calculateCdl3Outside(e);
                else if (Indicator == Indicators.Cdl3StarsInSouth)
                    calculateCdl3StarsInSouth(e);
                else if (Indicator == Indicators.Cdl3WhiteSoldiers)
                    calculateCdl3WhiteSoldiers(e);
                else if (Indicator == Indicators.CdlAdvanceBlock)
                    calculateCdlAdvanceBlock(e);
                else if (Indicator == Indicators.CdlBeltHold)
                    calculateCdlBeltHold(e);
                else if (Indicator == Indicators.CdlBreakaway)
                    calculateCdlBreakaway(e);
                else if (Indicator == Indicators.CdlClosingMarubozu)
                    calculateCdlClosingMarubozu(e);
                else if (Indicator == Indicators.CdlConcealBabysWall)
                    calculateCdlConcealBabysWall(e);
                else if (Indicator == Indicators.CdlCounterAttack)
                    calculateCdlCounterAttack(e);
                else if (Indicator == Indicators.CdlDoji)
                    calculateCdlDoji(e);
                else if (Indicator == Indicators.CdlDojiStar)
                    calculateCdlDojiStar(e);
                else if (Indicator == Indicators.CdlDragonflyDoji)
                    calculateCdlDragonflyDoji(e);
                else if (Indicator == Indicators.CdlEngulfing)
                    calculateCdlEngulfing(e);
                else if (Indicator == Indicators.CdlGapSideSideWhite)
                    calculateCdlGapSideSideWhite(e);
                else if (Indicator == Indicators.CdlGravestoneDoji)
                    calculateCdlGravestoneDoji(e);
                else if (Indicator == Indicators.CdlHammer)
                    calculateCdlHammer(e);
                else if (Indicator == Indicators.CdlHangingMan)
                    calculateCdlHangingMan(e);
                else if (Indicator == Indicators.CdlHarami)
                    calculateCdlHarami(e);
                else if (Indicator == Indicators.CdlHaramiCross)
                    calculateCdlHaramiCross(e);
                else if (Indicator == Indicators.CdlHignWave)
                    calculateCdlHignWave(e);
                else if (Indicator == Indicators.CdlHikkake)
                    calculateCdlHikkake(e);
                else if (Indicator == Indicators.CdlHikkakeMod)
                    calculateCdlHikkakeMod(e);
                else if (Indicator == Indicators.CdlHomingPigeon)
                    calculateCdlHomingPigeon(e);
                else if (Indicator == Indicators.CdlIdentical3Crows)
                    calculateCdlIdentical3Crows(e);
                else if (Indicator == Indicators.CdlInNeck)
                    calculateCdlInNeck(e);
                else if (Indicator == Indicators.CdlInvertedHammer)
                    calculateCdlInvertedHammer(e);
                else if (Indicator == Indicators.CdlKicking)
                    calculateCdlKicking(e);
                else if (Indicator == Indicators.CdlKickingByLength)
                    calculateCdlKickingByLength(e);
                else if (Indicator == Indicators.CdlLadderBottom)
                    calculateCdlLadderBottom(e);
                else if (Indicator == Indicators.CdlLongLeggedDoji)
                    calculateCdlLongLeggedDoji(e);
                else if (Indicator == Indicators.CdlLongLine)
                    calculateCdlLongLine(e);
                else if (Indicator == Indicators.CdlMarubozu)
                    calculateCdlMarubozu(e);
                else if (Indicator == Indicators.CdlMatchingLow)
                    calculateCdlMatchingLow(e);
                else if (Indicator == Indicators.CdlOnNeck)
                    calculateCdlOnNeck(e);
                else if (Indicator == Indicators.CdlPiercing)
                    calculateCdlPiercing(e);
                else if (Indicator == Indicators.CdlRickshawMan)
                    calculateCdlRickshawMan(e);
                else if (Indicator == Indicators.CdlRiseFall3Methods)
                    calculateCdlRiseFall3Methods(e);
                else if (Indicator == Indicators.CdlSeperatingLines)
                    calculateCdlSeperatingLines(e);
                else if (Indicator == Indicators.CdlShootingStar)
                    calculateCdlShootingStar(e);
                else if (Indicator == Indicators.CdlShortLine)
                    calculateCdlShortLine(e);
                else if (Indicator == Indicators.CdlSpinningTop)
                    calculateCdlSpinningTop(e);
                else if (Indicator == Indicators.CdlStalledPattern)
                    calculateCdlStalledPattern(e);
                else if (Indicator == Indicators.CdlStickSandwhich)
                    calculateCdlStickSandwhich(e);
                else if (Indicator == Indicators.CdlTakuri)
                    calculateCdlTakuri(e);
                else if (Indicator == Indicators.CdlTasukiGap)
                    calculateCdlTasukiGap(e);
                else if (Indicator == Indicators.CdlThrusting)
                    calculateCdlThrusting(e);
                else if (Indicator == Indicators.CdlTristar)
                    calculateCdlTristar(e);
                else if (Indicator == Indicators.CdlUnique3River)
                    calculateCdlUnique3River(e);
                else if (Indicator == Indicators.CdlUpsideGap2Crows)
                    calculateCdlUpsideGap2Crows(e);
                else if (Indicator == Indicators.CdlXSideGap3Methods)
                    calculateCdlXSideGap3Methods(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EXC: " + ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        public void addCondition(IndicatorConditions c, double value)
        {
            CellCondition condt = new CellCondition(this, c, value);
            listCondition.Children.Add(condt);
        }

        public void CandlePull(Object o, CandlePullEventArgs e)
        {
            if (e.Width == CandleWidth)
            {
                this.Dispatcher.Invoke(() =>
                {
                    SetGraphValues(e);
                });
            }
        }

        private void SetGraph(Indicators indicators)
        {
            switch (Indicator)
            {
                case Indicators.RSI:
                    var xyConfig = Mappers.Xy<Alerts.UI.Graphs.LineChartModel>()
                        .X(xyModel => xyModel.DateTime)
                        .Y(xyModel => xyModel.Value);

                    AxisY.MaxValue = 100;
                    AxisY.MinValue = 0;

                    SeriesCollection = new SeriesCollection(xyConfig);
                    SeriesCollection.Add(new LineSeries { Values = new ChartValues<LineChartModel>(), LineSmoothness = 0 });
                    break;
                default:
                    CartesianMapper<LineChartModel> xyConfigDefault = Mappers.Xy<Alerts.UI.Graphs.LineChartModel>()
                        .X(xyModel => xyModel.DateTime)
                        .Y(xyModel => xyModel.Value);

                    SeriesCollection = new SeriesCollection(xyConfigDefault);
                    SeriesCollection.Add(new LineSeries { Values = new ChartValues<LineChartModel>(), LineSmoothness = 0 });
                    break;
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Grid)((Button)sender).Content).Children[0].Visibility = Visibility.Visible;
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Grid)((Button)sender).Content).Children[0].Visibility = Visibility.Hidden;
        }
    }
}
