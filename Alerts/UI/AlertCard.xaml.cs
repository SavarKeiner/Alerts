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
        private const int maxPoints = 20;
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
                    if (step == 'm')
                    {
                        AxisStep = (long)TimeSpan.FromMinutes(int.Parse(value.ToString().Substring(1))).TotalMilliseconds * 4;
                        AxisUnit = TimeSpan.FromMinutes(int.Parse(value.ToString().Substring(1))).TotalMilliseconds * 4;
                    }
                    else if (step == 'h')
                    {
                        AxisStep = (long)TimeSpan.FromHours(int.Parse(value.ToString().Substring(1))).TotalMilliseconds * 4;
                        AxisUnit = TimeSpan.FromHours(int.Parse(value.ToString().Substring(1))).TotalMilliseconds * 4;
                    }
                    else if (step == 'd')
                    {
                        AxisStep = (long)TimeSpan.FromDays(int.Parse(value.ToString().Substring(1))).TotalMilliseconds * 4;
                        AxisUnit = TimeSpan.FromDays(int.Parse(value.ToString().Substring(1))).TotalMilliseconds * 4;
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

            this.CandleWidth = CandleWidth;
            this.Exchange = Exchange;
            this.Coin = Coin;
            this.Pair = Pair;
            this.Indicator = Indicator;
            this.alert = alert;

            btnRemoveCard.Click += (o, e) =>
            {
                alert.removeTo(this);
            };

            //15212094872460000000
            FormatterX = value => (new DateTime(1970, 1, 1)).AddMilliseconds((long)value).ToString("HH:mm");
            FormatterY = value => (value.ToString("0.00000000", CultureInfo.InvariantCulture));
            //LabelPoint = chartPoint => chartPoint.Y.ToString("0.00") + (new DateTime(1970, 1, 1)).AddMilliseconds(chartPoint.X).ToString("yyyy:MM:dd:HH:mm:ss");
            //Formatter = value => (new DateTime(1970, 1, 1)).AddTicks((long)value).ToString("HH:mm");
            DataContext = this;
        }

        private void calculateRsi()
        {

        }

        public void SetGraphValues(CandlePullEventArgs e)
        {
            try
            {
                List<CandleIF> list = e.candleList;
                ChartValues<LineChartModel> model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
                int optInTimePeriod = 14;
                int a;
                int b = 0;
                double[] c = new double[e.candleList.Count];

                if (Indicator == Indicators.RSI)
                {
                    model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
                    double[] closeArr = list.Select(x => x.getClose()).ToArray();

                    TicTacTec.TA.Library.Core.Rsi(0, e.candleList.Count - 1, closeArr, optInTimePeriod, out a, out b, c);

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
                            model[model.Count - 1].Value = c[200 - 1 - optInTimePeriod];
                        }
                        else if (last.getOpenTime() > clast.DateTime)
                        {
                            model.RemoveAt(0);
                            model.Add(new LineChartModel { DateTime = last.getOpenTime(), Value = c[200 - 1 - optInTimePeriod] });

                        }

                        /*foreach (LineChartModel m in model)
                        {
                            System.Diagnostics.Debug.WriteLine("rs: " + m.Value + " " + m.DateTime);
                        }*/
                    }

                    Value = c[200 - 1 - optInTimePeriod];
                    ValueChange = c[200 - 1 - optInTimePeriod] - c[200 - 2 - optInTimePeriod];
                }
                else if(Indicator == Indicators.PRICE)
                {
                    model = (ChartValues<LineChartModel>)((LineSeries)SeriesCollection[0]).Values;
                    double[] closeArr = list.Select(x => x.getClose()).ToArray();

                    if (model.Count == 0)
                    {
                        for (int i = 0; i < maxPoints; i++)
                        {
                            model.Add(new LineChartModel { DateTime = list[list.Count - 1 - maxPoints + i].getOpenTime(), Value = list[list.Count - 1 - maxPoints + i].getClose() });
                        }
                    }
                    else if (model.Count == maxPoints)
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

                        /*foreach (LineChartModel m in model)
                        {
                            System.Diagnostics.Debug.WriteLine("rs: " + m.Value + " " + m.DateTime);
                        }*/
                    }
                    Value = e.candleList[e.candleList.Count - 1].getClose();
                    ValueChange = e.candleList[e.candleList.Count - 1].getClose() - e.candleList[e.candleList.Count - 2].getClose();
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
                    SeriesCollection.Add(new LineSeries { Values = new ChartValues<LineChartModel>() });
                    break;
                case Indicators.PRICE:
                    var xyConfig1 = Mappers.Xy<Alerts.UI.Graphs.LineChartModel>()
                        .X(xyModel => xyModel.DateTime)
                        .Y(xyModel => xyModel.Value);


                    SeriesCollection = new SeriesCollection(xyConfig1);
                    SeriesCollection.Add(new LineSeries { Values = new ChartValues<LineChartModel>() });
                    break;
            }
        }

        private void Chart_DataHover(object sender, ChartPoint chartPoint)
        {

        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Grid)((Button)sender).Content).Children[0].Visibility = Visibility.Visible;
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Grid)((Button)sender).Content).Children[0].Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }


}
