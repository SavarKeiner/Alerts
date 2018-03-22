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


            if(Exchange == Exchanges.Binance)
            {
                exchangeIF = new Binance(this, Coin, Pair);
            }

            /*AlertCard card = new AlertCard();
            card.KlineWidth = KLineWidth.m5;
            card.Indicator = Indicators.RSI;
            card.Coin = Coins.ETH;
            card.Pair = Coins.BTC;
            this.addTo(card);*/


            Header.btnAdd.Click += AddClick;
        }

        public void addTo(AlertCard card)
        {
            exchangeIF.add(card, childList);

            CardGrid.Children.Add(card);
            SetPosition();
        }

        public void removeTo(AlertCard card)
        {
            exchangeIF.add(card, childList);

            CardGrid.Children.Remove(card);
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
                double y1 =  y * 300.0d;

                if (x >= maxXCards)
                    x = 0;

                Canvas.SetTop(childList[i], y1);
                Canvas.SetLeft(childList[i], x * 370);

                x++;
            }

            CardGrid.Height = 300 + y * 300;
            //System.Diagnostics.Debug.WriteLine("111: " + this.ActualHeight + " " + this.ActualWidth + " " + childList[0].ActualHeight + " " + childList[0].ActualWidth + " " + childList[0].griddd.ActualHeight + " " + this.grid.ActualHeight + " " + CardGrid.ActualHeight);
        }

        private void AddClick(Object o, RoutedEventArgs e)
        {
            CellGridAddPopup dialog = new CellGridAddPopup();
            dialog.Exchange = Exchange;
            dialog.Coin = Coin;


            dialog.ShowDialog();

            if (dialog.DialogResult.HasValue && dialog.DialogResult == true)
            {
                AlertCard card = new AlertCard(dialog.KlinesWidth, Exchange, Coin, Pair, dialog.Indicator);

                card.addCondition(dialog.Condition, dialog.ConditionValue);
                addTo(card);
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("DBG-Layout card: " + e.NewSize.Width + " " + e.NewSize.Height);
            SetPosition();
        }
    }
}
