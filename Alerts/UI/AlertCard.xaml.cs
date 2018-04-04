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
                        if(oldValue <= condition.value && newValue >= condition.value)
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

        private void calculateDx(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int optInTimePeriod = 14;

            model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;

            TicTacTec.TA.Library.Core.Dx(0, 200 - 1, e.candleList.Select(x => x.getHigh()).ToArray(), e.candleList.Select(x => x.getLow()).ToArray(), e.candleList.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (e.newList == true)
            {
                model.Clear();
                for (int i = 0; i < maxPoints; i++)
                {
                    model.Add(new LineChartModel { DateTime = e.candleList[200 - maxPoints + i].getOpenTime(), Value = c[b - maxPoints + i] });
                }
            }

            if (model.Count == 0)
            {
                for (int i = 0; i < maxPoints; i++)
                {
                    model.Add(new LineChartModel { DateTime = e.candleList[200 - maxPoints + i].getOpenTime(), Value = c[b - maxPoints + i] });
                }
            }
            else if (model.Count == maxPoints)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = e.candleList[e.candleList.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }

                /*foreach (LineChartModel m in model)
                {
                    System.Diagnostics.Debug.WriteLine("rs: " + m.Value + " " + m.DateTime);
                }*/
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], e.candleList[e.candleList.Count - 1].getOpenTime());
        }

        private void calculateAdx(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int optInTimePeriod = 14;

            model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;

            TicTacTec.TA.Library.Core.Adx(0, 200 - 1, e.candleList.Select(x => x.getHigh()).ToArray(), e.candleList.Select(x => x.getLow()).ToArray(), e.candleList.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (e.newList == true)
            {
                model.Clear();
                for (int i = 0; i < maxPoints; i++)
                {
                    model.Add(new LineChartModel { DateTime = e.candleList[200 - maxPoints + i].getOpenTime(), Value = c[b - maxPoints + i] });
                }
            }

            if (model.Count == 0)
            {
                for (int i = 0; i < maxPoints; i++)
                {
                    model.Add(new LineChartModel { DateTime = e.candleList[200 - maxPoints + i].getOpenTime(), Value = c[b - maxPoints + i] });
                }
            }
            else if (model.Count == maxPoints)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = e.candleList[e.candleList.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }

                /*foreach (LineChartModel m in model)
                {
                    System.Diagnostics.Debug.WriteLine("rs: " + m.Value + " " + m.DateTime);
                }*/
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], e.candleList[e.candleList.Count - 1].getOpenTime());
        }

        private void calculateCci(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];
            int optInTimePeriod = 14;

            model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;

            TicTacTec.TA.Library.Core.Cci(0, 200 - 1, e.candleList.Select(x => x.getHigh()).ToArray(), e.candleList.Select(x => x.getLow()).ToArray(), e.candleList.Select(x => x.getClose()).ToArray(), optInTimePeriod, out a, out b, c);

            if (e.newList == true)
            {
                model.Clear();
                for (int i = 0; i < maxPoints; i++)
                {
                    model.Add(new LineChartModel { DateTime = e.candleList[200 - maxPoints + i].getOpenTime(), Value = c[b - maxPoints + i] });
                }
            }

            if (model.Count == 0)
            {
                for (int i = 0; i < maxPoints; i++)
                {
                    model.Add(new LineChartModel { DateTime = e.candleList[200 - maxPoints + i].getOpenTime(), Value = c[b - maxPoints + i] });
                }
            }
            else if (model.Count == maxPoints)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = e.candleList[e.candleList.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }

                /*foreach (LineChartModel m in model)
                {
                    System.Diagnostics.Debug.WriteLine("rs: " + m.Value + " " + m.DateTime);
                }*/
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], e.candleList[e.candleList.Count - 1].getOpenTime());
        }

        private void calculateObv(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];

            model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;

            TicTacTec.TA.Library.Core.Obv(0, 200 - 1, e.candleList.Select(x => x.getClose()).ToArray(), e.candleList.Select(x => x.getVolume()).ToArray(), out a, out b, c);

            if (e.newList == true)
            {
                model.Clear();
                for (int i = 0; i < maxPoints; i++)
                {
                    model.Add(new LineChartModel { DateTime = e.candleList[200 - maxPoints + i].getOpenTime(), Value = c[b - maxPoints + i] });
                }
            }

            if (model.Count == 0)
            {
                for (int i = 0; i < maxPoints; i++)
                {
                    model.Add(new LineChartModel { DateTime = e.candleList[200 - maxPoints + i].getOpenTime(), Value = c[b - maxPoints + i] });
                }
            }
            else if (model.Count == maxPoints)
            {
                LineChartModel clast = model[model.Count - 1];
                CandleIF last = e.candleList[e.candleList.Count - 1];

                if (last.getOpenTime() == clast.DateTime)
                {
                    model[model.Count - 1].Value = c[b - 1];
                }
                else if (last.getOpenTime() > clast.DateTime)
                {
                    model.RemoveAt(0);
                    model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });

                }

                /*foreach (LineChartModel m in model)
                {
                    System.Diagnostics.Debug.WriteLine("rs: " + m.Value + " " + m.DateTime);
                }*/
            }

            Value = c[b - 1];
            ValueChange = (c[b - 1] - c[b - 2]);

            calcConditions(c[b - 2], c[b - 1], e.candleList[e.candleList.Count - 1].getOpenTime());
        }

        private void calculateTrix(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model;
            int optInTimePeriod = 18;
            int a;
            int b = 0;
            double[] c = new double[e.candleList.Count];

            model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            double[] closeArr = list.Select(x => x.getClose()).ToArray();

            TicTacTec.TA.Library.Core.Trix(0, e.candleList.Count - 1, closeArr, optInTimePeriod, out a, out b, c);

            if (e.newList == true)
            {
                model.Clear();
                for (int i = 0; i < maxPoints; i++)
                {
                    model.Add(new LineChartModel { DateTime = e.candleList[200 - maxPoints + i].getOpenTime(), Value = c[b - maxPoints + i] * 100 });
                }
            }

            if (model.Count == 0)
            {
                for (int i = 0; i < maxPoints; i++)
                {
                    model.Add(new LineChartModel { DateTime = e.candleList[200 - maxPoints + i].getOpenTime(), Value = c[b - maxPoints + i] * 100 });
                }
            }
            else if (model.Count == maxPoints)
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

                /*foreach (LineChartModel m in model)
                {
                    System.Diagnostics.Debug.WriteLine("rs: " + m.Value + " " + m.DateTime);
                }*/
            }

            Value = c[b - 1] * 100;
            ValueChange = (c[b - 1] - c[b -2]) * 100;

            calcConditions(c[b - 2] * 100, c[b - 1] * 100, e.candleList[e.candleList.Count - 1].getOpenTime());
        }

        private void calculateRsi(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            int optInTimePeriod = 14;
            int a = 0;
            int b = 0;
            double[] c = new double[list.Count];
            int pointsToDraw = App.Clamp(list.Count - optInTimePeriod , 0, maxPoints); //if there are less points to draw then set in maxPoints

            if(list.Count <= optInTimePeriod) //check if indicator can even be calculated, coin has less candles than required for optInTimePeriod
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
            else if(model.Count < pointsToDraw)
            {
                CandleIF last = list[list.Count - 1];
                model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[b - 1] });
            }

            Value = c[b -1];
            ValueChange = c[b -1] - c[b - 2];

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

            Value = e.candleList[e.candleList.Count - 1].getClose();
            ValueChange = e.candleList[e.candleList.Count - 1].getClose() - e.candleList[e.candleList.Count - 2].getClose();

            if (ValueChange >= 0)
                indicatorChange.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));
            else
                indicatorChange.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));

            calcConditions(list[list.Count - 2].getClose(), list[list.Count - 1].getClose(), list[list.Count - 1].getOpenTime());
        }

        private void calculateVolume(CandlePullEventArgs e)
        {
            List<CandleIF> list = e.candleList;
            ChartValues<LineChartModel> model;

            model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
            double[] volArr = list.Select(x => x.getVolume()).ToArray();

            if (e.newList == true)
            {
                model.Clear();
                for (int i = 0; i < maxPoints; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - 1 - maxPoints + i].getOpenTime(), Value = list[list.Count - 1 - maxPoints + i].getVolume() });
                }
            }

            if (model.Count == 0)
            {
                for (int i = 0; i < maxPoints; i++)
                {
                    model.Add(new LineChartModel { DateTime = list[list.Count - 1 - maxPoints + i].getOpenTime(), Value = list[list.Count - 1 - maxPoints + i].getVolume() });
                }
            }
            else if (model.Count == maxPoints)
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

                /*foreach (LineChartModel m in model)
                {
                    System.Diagnostics.Debug.WriteLine("rs: " + m.Value + " " + m.DateTime);
                }*/
            }
            Value = e.candleList[e.candleList.Count - 1].getVolume();
            ValueChange = e.candleList[e.candleList.Count - 1].getVolume() - e.candleList[e.candleList.Count - 2].getVolume();

            if (ValueChange >= 0)
                indicatorChange.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));
            else
                indicatorChange.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));

            calcConditions(e.candleList[e.candleList.Count - 2].getClose(), e.candleList[e.candleList.Count - 1].getClose(), e.candleList[e.candleList.Count - 1].getOpenTime());
        }



        public void SetGraphValues(CandlePullEventArgs e)
        {
            try
            {
                if (Indicator == Indicators.RSI)
                {
                    calculateRsi(e);
                }
                else if(Indicator == Indicators.PRICE)
                {
                    calculatePrice(e);
                }
                else if (Indicator == Indicators.VOLUME)
                {
                    calculateVolume(e);
                }
                else if (Indicator == Indicators.TRIX)
                {
                    calculateTrix(e);
                }
                else if (Indicator == Indicators.OBV)
                {
                    calculateObv(e);
                }
                else if (Indicator == Indicators.CCI)
                {
                    calculateCci(e);
                }
                else if (Indicator == Indicators.ADX)
                {
                    calculateAdx(e);
                }
                else if (Indicator == Indicators.DX)
                {
                    calculateDx(e);
                }

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
                case Indicators.PRICE:
                    var xyConfig1 = Mappers.Xy<Alerts.UI.Graphs.LineChartModel>()
                        .X(xyModel => xyModel.DateTime)
                        .Y(xyModel => xyModel.Value);

                    SeriesCollection = new SeriesCollection(xyConfig1);
                    SeriesCollection.Add(new LineSeries { Values = new ChartValues<LineChartModel>(), LineSmoothness = 0 });
                    break;
                case Indicators.VOLUME:
                    var xyConfig2 = Mappers.Xy<Alerts.UI.Graphs.LineChartModel>()
                        .X(xyModel => xyModel.DateTime)
                        .Y(xyModel => xyModel.Value);

                    SeriesCollection = new SeriesCollection(xyConfig2);
                    SeriesCollection.Add(new LineSeries { Values = new ChartValues<LineChartModel>(), LineSmoothness = 0 });
                    break;
                case Indicators.TRIX:
                    var xyConfig3 = Mappers.Xy<Alerts.UI.Graphs.LineChartModel>()
                        .X(xyModel => xyModel.DateTime)
                        .Y(xyModel => xyModel.Value);

                    SeriesCollection = new SeriesCollection(xyConfig3);
                    SeriesCollection.Add(new LineSeries { Values = new ChartValues<LineChartModel>(), LineSmoothness = 0 });
                    break;
                case Indicators.OBV:
                    var xyConfig4 = Mappers.Xy<Alerts.UI.Graphs.LineChartModel>()
                        .X(xyModel => xyModel.DateTime)
                        .Y(xyModel => xyModel.Value);

                    SeriesCollection = new SeriesCollection(xyConfig4);
                    SeriesCollection.Add(new LineSeries { Values = new ChartValues<LineChartModel>(), LineSmoothness = 0 });
                    break;
                case Indicators.CCI:
                    CartesianMapper<LineChartModel> xyConfig5 = Mappers.Xy<Alerts.UI.Graphs.LineChartModel>()
                        .X(xyModel => xyModel.DateTime)
                        .Y(xyModel => xyModel.Value);

                    SeriesCollection = new SeriesCollection(xyConfig5);
                    SeriesCollection.Add(new LineSeries { Values = new ChartValues<LineChartModel>(), LineSmoothness = 0 });
                    break;
                case Indicators.ADX:
                    CartesianMapper<LineChartModel> xyConfig6 = Mappers.Xy<Alerts.UI.Graphs.LineChartModel>()
                        .X(xyModel => xyModel.DateTime)
                        .Y(xyModel => xyModel.Value);

                    SeriesCollection = new SeriesCollection(xyConfig6);
                    SeriesCollection.Add(new LineSeries { Values = new ChartValues<LineChartModel>(), LineSmoothness = 0 });
                    break;
                case Indicators.DX:
                    CartesianMapper<LineChartModel> xyConfig7 = Mappers.Xy<Alerts.UI.Graphs.LineChartModel>()
                        .X(xyModel => xyModel.DateTime)
                        .Y(xyModel => xyModel.Value);

                    SeriesCollection = new SeriesCollection(xyConfig7);
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
