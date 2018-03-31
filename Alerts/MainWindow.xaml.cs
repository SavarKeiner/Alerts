using Alerts.Logic.Enums;
using Alerts.UI;
using System;
using System.Collections.Generic;
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

namespace Alerts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            sideBar.Visibility = Visibility.Collapsed;
            sideBar.gridExchange.Visibility = Visibility.Collapsed;
            sideBar.gridPairing.Visibility = Visibility.Collapsed;
            sideBar.gridCoin.Visibility = Visibility.Collapsed;
            sideBar.gridCondition.Visibility = Visibility.Collapsed;

            //listCellCoin.Children.Add(new AlertLayout { Exchange = Logic.Enums.Exchanges.Binance, Coin = Logic.Enums.Coins.ETH, Pair = Logic.Enums.Coins.BTC});
        }

        public void addIndicator()
        {

        }

        public void removeCellCoin()
        {

        }

        public void reSize()
        {
            AlertLayout lastCellCoin = null;
            double lastY = 0;
            foreach (AlertLayout cc in listCellCoin.Children)
            {

                Canvas.SetTop(cc, lastY);
                cc.Width = listCellCoin.ActualWidth;


                cc.Width = listCellCoin.ActualWidth;
                lastCellCoin = cc;
                lastY += cc.ActualHeight;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("MainSize: " + e.NewSize.Width + " " + sideMenu.ActualWidth + " " + listCellCoin.ActualWidth + " " + Scroll.ActualWidth);

            Scroll.Width = e.NewSize.Width - sideMenu.ActualWidth - 16;

            Canvas.SetTop(notificationArea, mW.ActualHeight - notificationArea.ActualHeight);
            notificationArea.Width = e.NewSize.Width - sideMenu.ActualWidth - 16;
            darkener.Width = e.NewSize.Width - sideMenu.ActualWidth - 16;
            //listCellCoin.Width = e.NewSize.Width - sideMenu.ActualWidth - 16;
            reSize();
        }

        public void addIndicator(Exchanges selectedExchange, Coins selectedCoin, Coins selectedPairing, CandleWidth selectedWidth, Indicators selectedIndicator, IndicatorConditions selectedCondition, double value)
        {
            foreach(AlertLayout l in listCellCoin.Children) //in every current layout
            {
                if(l.Coin == selectedCoin && l.Pair == selectedPairing && l.Exchange == selectedExchange)// if layout exists with params
                {
                    foreach(AlertCard c in l.CardGrid.Children) //search for card with same indicator exists
                    {
                        if (c.Indicator == selectedIndicator && c.CandleWidth == selectedWidth) //if card with same indicator and candle width exists
                        {
                            c.addCondition(selectedCondition, value); //just needed to add condition, else it is a new card
                            return;
                        }
                    }
                    //indicator was not found in card list, need to create new one
                    AlertCard _card = new AlertCard(l, selectedWidth, selectedExchange, selectedCoin, selectedPairing, selectedIndicator);
                    _card.addCondition(selectedCondition, value);
                    l.addTo(_card);
                    return;
                }

            }

            AlertLayout alert = new AlertLayout(selectedExchange, selectedCoin, selectedPairing);
            AlertCard card = new AlertCard(alert, selectedWidth, selectedExchange, selectedCoin, selectedPairing, selectedIndicator);
            card.addCondition(selectedCondition, value);
            alert.addTo(card);
            listCellCoin.Children.Add(alert);
            reSize();
        }

        private void mW_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("shit1: " + e.NewSize.Width);
            //Scroll.Width = e.NewSize.Width - sideMenu.ActualWidth - 16;
            //reSize();
        }
    }
}
