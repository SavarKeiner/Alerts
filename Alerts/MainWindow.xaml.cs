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

        public void addCellCoin()
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
            //System.Diagnostics.Debug.WriteLine("shit: " + e.NewSize.Width + " " + sideMenu.ActualWidth);

            listCellCoin.Width = e.NewSize.Width - sideMenu.ActualWidth - 16;
            reSize();
        }
    }
}
