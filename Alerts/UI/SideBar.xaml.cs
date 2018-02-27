using Alerts.Logic.Enums;
using Alerts.UI.Dialogs;
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

namespace Alerts.UI
{
    /// <summary>
    /// Interaction logic for SideBar.xaml
    /// </summary>
    public partial class SideBar : UserControl
    {
        List<ImageTextItem> l = new List<ImageTextItem>();
        private Exchanges selectedExchange;
        private Coins selectedCoin;

        public SideBar()
        {
            InitializeComponent();

            ImageTextItem imageTextItem1 = new ImageTextItem();
            imageTextItem1.image.Source = new Uri("/UI/Icons/ExchangeIcons/binance.svg", UriKind.Relative);
            imageTextItem1.text.Content = "Binance";

            l.Add(imageTextItem1);
            listExchange.ItemsSource = l;

            this.IsVisibleChanged += (o, e) => {
                bool isV = (bool)e.NewValue;

                if (isV == true)
                {
                    parentGrid.Width = 150;
                    listExchange.SelectedItem = null;
                    listCoins.ItemsSource = null;
                }
                else if (isV == false)
                {
                    parentGrid.Width = 150;
                    listExchange.SelectedItem = null;
                    selectedExchange = Exchanges.INIT;
                    selectedCoin = Coins.INTI;
                }
            };
        }

        private void listExchange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(((ListBox)e.Source).SelectedItem != null)
            {
                Exchanges ex = (Exchanges)Enum.Parse(typeof(Exchanges), (string)((ImageTextItem)((ListBox)e.Source).SelectedItem).text.Content);
                if(ex == Exchanges.Binance)
                {
                    selectedExchange = ex;
                    setBinanceCoins();
                }

                parentGrid.Width = 300;
            }
        }

        private void listCoins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)e.Source).SelectedItem != null)
            {
                selectedCoin = (Coins)Enum.Parse(typeof(Coins), (string)((ImageTextItem)((ListBox)e.Source).SelectedItem).text.Content);

                CellGridAddPopup dialog = new CellGridAddPopup();
                dialog.Exchange = selectedExchange;
                dialog.Coin = selectedCoin;


                dialog.ShowDialog();

                if (dialog.DialogResult.HasValue && dialog.DialogResult == true)
                {
                    Application curApp = Application.Current;
                    MainWindow mainWindow = (MainWindow)curApp.MainWindow;

                    CellCoin cellCoin = new CellCoin(selectedCoin, selectedExchange);

                    cellCoin.cellGrid.addIndicator(dialog.KlinesWidth, dialog.Indicator, dialog.Condition, dialog.ConditionValue);

                    mainWindow.listCellCoin.Children.Add(cellCoin);
                    this.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void setBinanceCoins()
        {
            List<ImageTextItem> list = new List<ImageTextItem>();

            ImageTextItem imageTextItem1 = new ImageTextItem();
            imageTextItem1.image.Source = new Uri("/UI/Icons/CoinIcons/bitcoin.svg", UriKind.Relative);
            imageTextItem1.text.Content = Coins.Bitcoin.ToString();

            list.Add(imageTextItem1);
            listCoins.ItemsSource = list;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("CLICK");
        }
    }
}
