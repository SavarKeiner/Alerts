using Alerts.Logic.Enums;
using Alerts.UI.Dialogs;
using Newtonsoft.Json.Linq;
using RestSharp;
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
        List<ImageTextItem> exchangeSource = new List<ImageTextItem>();
        List<ImageTextItem> pairingSourceBinance = new List<ImageTextItem>();
        List<ImageTextItem> coineSourceBinance = new List<ImageTextItem>();

        private Dictionary<Coins, List<Coins>> paringCoinDict = new Dictionary<Coins, List<Coins>>();

        public Exchanges selectedExchange { get; set; }
        public Coins selectedPairing { get; set; }
        public Coins selectedCoin { get; set; }



        public SideBar()
        {
            InitializeComponent();

            ImageTextItem imageTextItem1 = new ImageTextItem();
            imageTextItem1.image.Source = new Uri("/UI/Icons/ExchangeIcons/binance.svg", UriKind.Relative);
            imageTextItem1.text.Content = "Binance";

            exchangeSource.Add(imageTextItem1);
            listExchange.ItemsSource = exchangeSource;

            gridExchange.IsVisibleChanged += (o, e) =>
            {
                if((bool)e.NewValue == true)
                {
                    parentGrid.Width = 150;
                }
                else
                {
                    listExchange.UnselectAll();
                    parentGrid.Width = 0;
                }
            };

            gridPairing.IsVisibleChanged += (o, e) =>
            {
                if ((bool)e.NewValue == true)
                {
                    parentGrid.Width = 300;
                }
                else
                {
                    listPairing.UnselectAll();
                    parentGrid.Width = 150;
                }
            };

            gridCoin.IsVisibleChanged += (o, e) =>
            {
                if ((bool)e.NewValue == true)
                {
                    parentGrid.Width = 450;
                }
                else
                {
                    listCoins.UnselectAll();
                    parentGrid.Width = 300;
                }
            };
            /*this.IsVisibleChanged += (o, e) => {
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
            };*/
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


                gridPairing.Visibility = Visibility.Visible;
            }
        }

        private void listPairing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)e.Source).SelectedItem != null)
            {
                selectedPairing = (Coins)Enum.Parse(typeof(Coins), ((ImageTextItem)((ListBox)e.Source).SelectedItem).text.Content.ToString());
                gridCoin.Visibility = Visibility.Visible;

                List<Coins> lc = new List<Coins>();
                paringCoinDict.TryGetValue(selectedPairing, out lc);

                coineSourceBinance.Clear();
                foreach (Coins c in lc)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("DBG-EX: " + c);

                        ImageTextItem item = new ImageTextItem();
                        item.image.Source = new Uri("/UI/Icons/CoinIcons/" + c + ".svg", UriKind.Relative);
                        item.text.Content = c;

                        coineSourceBinance.Add(item);
                    } catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("DBG-EX: " + ex.Message + " " + ex.StackTrace);
                    }
                }
                listCoins.ItemsSource = null;
                listCoins.ItemsSource = coineSourceBinance;
            }
        }

        private void listCoins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)e.Source).SelectedItem != null)
            {
                selectedCoin = (Coins)Enum.Parse(typeof(Coins), ((ImageTextItem)((ListBox)e.Source).SelectedItem).text.Content.ToString());

                CellGridAddPopup dialog = new CellGridAddPopup();
                dialog.Exchange = selectedExchange;
                dialog.Coin = selectedCoin;


                dialog.ShowDialog();

                if (dialog.DialogResult.HasValue && dialog.DialogResult == true)
                {
                    Application curApp = Application.Current;
                    MainWindow mainWindow = (MainWindow)curApp.MainWindow;

                    CellCoin cellCoin = new CellCoin(selectedExchange, selectedPairing, selectedCoin);

                    cellCoin.cellGrid.addIndicator(dialog.KlinesWidth, dialog.Indicator, dialog.Condition, dialog.ConditionValue);


                    this.Visibility = Visibility.Collapsed;
                    gridCoin.Visibility = Visibility.Collapsed;
                    gridPairing.Visibility = Visibility.Collapsed;
                    gridExchange.Visibility = Visibility.Collapsed;
                    mainWindow.listCellCoin.Children.Add(cellCoin);
                }

                listCoins.UnselectAll();
            }
        }

        private void setBinanceCoins()
        {
            //todo: needs try catch over coin/pair
            if (listPairing.ItemsSource != null)
                return;

            RestClient client = new RestClient("https://api.binance.com");
            RestRequest request = new RestRequest("api/v1/exchangeInfo");

            IRestResponse response = client.Execute(request);

            JToken jt = JToken.Parse(response.Content).SelectToken("symbols");

            foreach (JToken token in jt)
            {
                if(token.SelectToken("quoteAsset").ToString() == "456")//dont know why this pairing exists
                    continue;

                try
                {
                    Coins k = (Coins)Enum.Parse(typeof(Coins), token.SelectToken("quoteAsset").ToString());
                    Coins v = (Coins)Enum.Parse(typeof(Coins), token.SelectToken("baseAsset").ToString());

                    if (!paringCoinDict.ContainsKey(k))
                    {
                        List<Coins> lv = new List<Coins>();
                        lv.Add(v);

                        paringCoinDict.Add(k, lv);
                    }
                    else
                    {
                        List<Coins> lv = new List<Coins>();
                        paringCoinDict.TryGetValue(k, out lv);
                        lv.Add(v);
                    }
                    
                    //paringCoinDict.Add((Coins)Enum.Parse(typeof(Coins), token.SelectToken("quoteAsset").ToString()), (Coins)Enum.Parse(typeof(Coins), token.SelectToken("baseAsset").ToString()));
                } catch(Exception e)
                {
                    //do nothing for now
                }
                
            }
            foreach(KeyValuePair<Coins, List<Coins>> entry in paringCoinDict)
            {
                ImageTextItem item = new ImageTextItem();
                item.image.Source = new Uri("/UI/Icons/CoinIcons/" + entry.Key + ".svg", UriKind.Relative);
                item.text.Content = entry.Key;

                pairingSourceBinance.Add(item);
            }
            listPairing.ItemsSource = pairingSourceBinance;
        }
    }
}
