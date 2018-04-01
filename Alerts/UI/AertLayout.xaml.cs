using Alerts.Logic.Enums;
using Alerts.Logic.Events;
using Alerts.Logic.ExchangeCode;
using Alerts.Logic.Interfaces;
using Alerts.UI.Dialogs;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for Alert.xaml
    /// </summary>
    public partial class AlertLayout : UserControl
    {
        public Exchanges Exchange { get; set; }
        public Coins Coin { get; set; }
        public Coins Pair { get; set; }

        private List<AlertCard> childList;

        private ExchangeIF exchangeIF;

        public AlertLayout(Exchanges Exchange, Coins Coin, Coins Pair)
        {
            childList = new List<AlertCard>();
            InitializeComponent();

            this.Exchange = Exchange;
            this.Coin = Coin;
            this.Pair = Pair;

            Header.alert = this;
            Header.Exchange = Exchange;
            Header.Symbol = Coin.ToString() + Pair.ToString();

            if (Exchange == Exchanges.Binance)
            {
                Binance b = Binance.Instance;
                exchangeIF = b;
            }

            /*AlertCard card = new AlertCard();
            card.KlineWidth = KLineWidth.m5;
            card.Indicator = Indicators.RSI;
            card.Coin = Coins.ETH;
            card.Pair = Coins.BTC;
            this.addTo(card);*/

        }

        public void add(CandleWidth selectedWidth, Indicators selectedIndicator, IndicatorConditions selectedCondition, double value)
        {

            foreach (AlertCard c in CardGrid.Children) //search for card with same indicator exists
            {
                if (c.Indicator == selectedIndicator && c.CandleWidth == selectedWidth) //if card with same indicator and candle width exists
                {
                    c.addCondition(selectedCondition, value); //just needed to add condition, else it is a new card
                    return;
                }
            }

            AlertCard card = new AlertCard(this, selectedWidth, Exchange, Coin, Pair, selectedIndicator);
            card.addCondition(selectedCondition, value);
            addTo(card);
            SetPosition();
        }

        public void addTo(AlertCard card)
        {
            if(childList.Count == 0)
            {
                Header.PullData(Exchange, Coin, Pair);
            }

            exchangeIF.add(card, Coin, Pair, childList);
            CardGrid.Children.Add(card);
            SetPosition();
        }

        public void removeTo(AlertCard card)
        {
            exchangeIF.remove(card, childList);
            CardGrid.Children.Remove(card);

            if(childList.Count == 0)
            {
                Application curApp = Application.Current;
                MainWindow mainWindow = (MainWindow)curApp.MainWindow;

                Header.source.Cancel();
                mainWindow.listCellCoin.Children.Remove(this);
            }

            SetPosition();
        }

        private void SetPosition()
        {
            if (this.ActualWidth == 0)
                return;

            double width = this.ActualWidth;

            int maxXCards = App.Clamp((int)Math.Floor(width / (double)370), 2, 99);
            int curx = childList.Count * 370;


            int x = 0;
            int y = 0;
            for (int i = 0; i < childList.Count; i++)
            {
                y = (int)Math.Floor(i / (double)maxXCards);
                double y1 = y * 300.0d;

                if (x >= maxXCards)
                    x = 0;

                Canvas.SetTop(childList[i], y1);
                Canvas.SetLeft(childList[i], x * 370);

                x++;
            }

            CardGrid.Height = 300 + y * 300;
            //System.Diagnostics.Debug.WriteLine("111: " + this.ActualHeight + " " + this.ActualWidth + " " + childList[0].ActualHeight + " " + childList[0].ActualWidth + " " + childList[0].griddd.ActualHeight + " " + this.grid.ActualHeight + " " + CardGrid.ActualHeight);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("DBG-Layout card: " + e.NewSize.Width + " " + e.NewSize.Height);
            SetPosition();
        }
    }
}
