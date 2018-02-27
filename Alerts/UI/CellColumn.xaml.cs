using Alerts.Logic.Enums;
using Alerts.Logic.Events;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for CellColumn.xaml
    /// </summary>
    public partial class CellColumn : UserControl
    {
        public CandleStickWidth KlineWidth { get; set; }
        public Indicators Indicator { get; set; }
        public Exchanges Exchange { get; set; }
        public Coins Coin { get; set; }
        public CellGrid cellGrid;
        public EventHandler<KlinesPulledEventArgs> KlinesPulled;
        private const int limit = 100;
        private bool shoudStop = false;

        public CellColumn()
        {
            InitializeComponent();
        }

        public CellColumn(CellGrid cellGrid, CandleStickWidth width, Indicators indicator, Exchanges exchange, Coins coin)
        {
            InitializeComponent();
            labelCandlestickWidth.Content = App.candleStickWidthToString(width);
            KlineWidth = width;
            Indicator = indicator;
            Exchange = exchange;
            Coin = coin;
            this.cellGrid = cellGrid;

            //this.KlinesPulled += (o, e) => { };

            pullKlines();
        }

        private async void pullKlines()
        {
            Stopwatch stopWatch = new Stopwatch();


            await Task.Run(() => {
                stopWatch.Start();

                while (!shoudStop)
                {
                    RestClient client = new RestClient("https://api.binance.com");
                    RestRequest request = new RestRequest("api/v1/klines?symbol=BTCUSDT&interval=" + App.candleStickWidthToString(KlineWidth) + "&limit=" + limit);

                    IRestResponse response = client.Execute(request);

                    JArray jArray = JArray.Parse(response.Content);
                    double[] close = new double[jArray.Count];

                    for (int i = 0; i < jArray.Count; i++)
                    {
                        JToken token = jArray[i];

                        string closePriceAsString = token[4].ToString();
                        double closePrice = Double.Parse(closePriceAsString, CultureInfo.InvariantCulture);
                        close[i] = closePrice;
                    }

                    JToken last = jArray[jArray.Count - 1];
                    KlinesPulledEventArgs args = new KlinesPulledEventArgs();
                    args.klinesArray = jArray;
                    args.close = close;
                    args.Lenght = jArray.Count;
                    args.lastTime = long.Parse(last[0].ToString());
                    OnKlinesPulled(args);

                    //System.Diagnostics.Debug.WriteLine("DBG-24: " + close[limit -1] + " " + close[288-1] + " " + close[limit - 1] / close[288 - 1]);
                    //System.Diagnostics.Debug.WriteLine("DBG-Async: " + stopWatch.ElapsedMilliseconds + " " + response.Content + " " + close[0] + " " + long.Parse(last[0].ToString()));
                    stopWatch.Restart();
                }
            });

            /*double[] close = new double[limit];
*/
        }

        public void addIndicator(CandleStickWidth candleStickWidth, Indicators indicator, IndicatorConditions condition, double value)
        {
            foreach(CellIndicator cellIndicator in indicatorList.Children)
            {
                if ((string)cellIndicator.indicatorHeader.labelIndicatorName.Content == indicator.ToString())
                {
                    cellIndicator.addCondition(condition, value);
                    return;
                }
            }

            CellIndicator ci = new CellIndicator(this, candleStickWidth, indicator);
            ci.addCondition(condition, value);
            indicatorList.Children.Add(ci);
        }

        public void removeIndicator(CellIndicator indicator)
        {
            indicatorList.Children.Remove(indicator);

            if(indicatorList.Children.Count == 0)
            {
                shoudStop = true;
                cellGrid.removeWhenColumnEmpty(this);
            }


            cellGrid.setPosition();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        protected virtual void OnKlinesPulled(KlinesPulledEventArgs e)
        {
            KlinesPulled?.Invoke(this, e);
        }

        ~CellColumn()
        {
            shoudStop = true;
        }
    }
}
