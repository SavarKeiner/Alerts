using Alerts.Logic.Enums;
using Alerts.Logic.Events;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for CellIndicator.xaml
    /// </summary>
    public partial class CellIndicator : UserControl
    {
        /*public CellColumn cellColumn;
        public CandleWidth candlestickWidth;
        public Indicators indicator;
        private const int optInTimePeriod = 14;

        public CellIndicator()
        {
            InitializeComponent();
        }

        public CellIndicator(CellColumn cellColumn, CandleWidth candlestickWidth,Indicators indicator)
        {
            InitializeComponent();

            indicatorHeader.indicator = indicator;
            indicatorHeader.cellIndicator = this;

            this.cellColumn = cellColumn;
            this.candlestickWidth = candlestickWidth;
            this.indicator = indicator;

            cellColumn.KlinesPulled += calcIndicators;
        }

        public void removeAll()
        {
            conditionList.Children.RemoveRange(0, conditionList.Children.Count);
            cellColumn.removeIndicator(this);
        }

        public void addCondition(IndicatorConditions condition, double value)
        {
            CellCondition cellCondition = new CellCondition(this, candlestickWidth, condition, value);
            conditionList.Children.Add(cellCondition);

            cellColumn.cellGrid.setPosition();

        }

        public void removeCondition(CellCondition removeMe)
        {
            conditionList.Children.Remove(removeMe);
            cellColumn.cellGrid.setPosition();
        }

        private void calcIndicators(Object sender, CandlePulledEventArgs e)
        {
            this.Dispatcher.Invoke(() => {
                if (indicator == Indicators.RSI)
                {
                    int a;
                    int b;
                    double[] c = new double[e.Lenght];
                    TicTacTec.TA.Library.Core.Rsi(0, e.Lenght - 1, e.close, optInTimePeriod, out a, out b, c);

                    if(e.Lenght < optInTimePeriod + 2)
                    {
                        System.Diagnostics.Debug.WriteLine("DBG-ERROR: lenght smaller optintimeperiod");
                    }
                    else
                    {
                        indicatorHeader.PValue = Math.Round(c[e.Lenght - optInTimePeriod - 1], 2);
                        indicatorHeader.PValueChange = Math.Round(c[e.Lenght - optInTimePeriod - 1] - c[e.Lenght - optInTimePeriod - 2], 2);

                        checkConditions(indicatorHeader.PValue, indicatorHeader.PValue + indicatorHeader.PValueChange);
                    }


                } else if (indicator == Indicators.PRICE)
                {
                    indicatorHeader.PValue = e.close[e.Lenght  - 1];
                    indicatorHeader.PValueChange = e.close[e.Lenght - 1] - e.close[e.Lenght - 2];

                    checkConditions(indicatorHeader.PValue, indicatorHeader.PValue + indicatorHeader.PValueChange);
                } 
                else if(indicator == Indicators.VOLUME)
                {
                    JToken tokenLast = e.klinesArray[e.klinesArray.Count - 1];
                    JToken tokenPrev = e.klinesArray[e.klinesArray.Count - 2];

                    double volumeLast = Math.Round(double.Parse(tokenLast[5].ToString(), CultureInfo.InvariantCulture), 2);
                    double volumePrev = Math.Round(double.Parse(tokenPrev[5].ToString(), CultureInfo.InvariantCulture), 2);

                    indicatorHeader.PValue = volumeLast;
                    indicatorHeader.PValueChange = volumeLast - volumePrev;

                    checkConditions(volumeLast, volumePrev);
                }
            });
        }

        private void checkConditions(double value, double prev)
        {
            foreach(CellCondition c in conditionList.Children)
            {
                switch (c.indicatorCondition)
                {
                    case IndicatorConditions.ABOVE:
                        if (value >= c.value)
                        {
                            //do somethign
                        }
                        break;
                    case IndicatorConditions.BELOW:
                        if (value <= c.value)
                        {
                            //do something
                        }
                        break;
                    case IndicatorConditions.CROSS:
                        if (value > c.value)
                        {
                            if (value > prev)
                            {
                                //do something
                            }
                        }
                        else
                        {
                            if (value < prev)
                            {
                                //do something
                            }
                        }
                        break;
                    case IndicatorConditions.CHANGE:
                        //do something

                        break;

                }  
            }
        }

        ~CellIndicator()
        {
            cellColumn.KlinesPulled -= calcIndicators;
        }*/
    }
}
