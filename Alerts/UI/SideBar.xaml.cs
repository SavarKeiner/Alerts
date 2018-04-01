using Alerts.Logic.Enums;
using Alerts.Logic.ExchangeCode;
using Alerts.Logic.Interfaces;
using Alerts.UI.Dialogs;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        List<ImageTextItem> exchangeSource = new List<ImageTextItem>(); // list of exchanges

        //List<ImageTextItem> pairingSourceBinance = new List<ImageTextItem>(); //list of pair for sp
        //List<ImageTextItem> coineSourceBinance = new List<ImageTextItem>(); //list of coin
        //private Dictionary<Coins, List<Coins>> paringCoinDict = new Dictionary<Coins, List<Coins>>();

        public Exchanges selectedExchange { get; set; }
        public Coins selectedPairing { get; set; }
        public Coins selectedCoin { get; set; }

        private Indicators selectedIndicator { get; set; } = Indicators.PRICE;
        private IndicatorConditions selectedCondition { get; set; } = IndicatorConditions.CROSS;
        private CandleWidth selectedWidth { get; set; } = CandleWidth.m5;
        private double ConditionValue { get; set; } = 0;

        private Binance binance = Binance.Instance;
        private ExchangeIF exchange;

        public SideBar()
        {
            InitializeComponent();

            ImageTextItem entryBinance = new ImageTextItem();
            entryBinance.image.Source = new Uri("/UI/Icons/ExchangeIcons/binance.svg", UriKind.Relative);
            entryBinance.text.Content = "Binance";

            exchangeSource.Add(entryBinance);
            listExchange.ItemsSource = exchangeSource;

            gridExchange.IsVisibleChanged += (o, e) =>
            {
                if ((bool)e.NewValue == true)
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

            gridCondition.IsVisibleChanged += (o, e) =>
            {
                if ((bool)e.NewValue == true)
                {
                    parentGrid.Width = 600;
                }
                else
                {
                    listCoins.UnselectAll();
                    parentGrid.Width = 450;
                }
            };
        }

        private void listExchange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)e.Source).SelectedItem != null)
            {
                Exchanges ex = (Exchanges)Enum.Parse(typeof(Exchanges), (string)((ImageTextItem)((ListBox)e.Source).SelectedItem).text.Content);
                if (ex == Exchanges.Binance)
                {
                    selectedExchange = ex;
                    exchange = binance;
                }

                List<ImageTextItem> list = null;

                if (list == null || list.Count == 0)
                {
                    list = exchange.GetPairList();
                }

                listPairing.ItemsSource = null;
                listPairing.ItemsSource = list;

                gridPairing.Visibility = Visibility.Visible;
                initSelectionCondition();
            }
        }

        private void listPairing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)e.Source).SelectedItem != null)
            {
                selectedPairing = (Coins)Enum.Parse(typeof(Coins), ((ImageTextItem)((ListBox)e.Source).SelectedItem).text.Content.ToString());
                gridCoin.Visibility = Visibility.Visible;


                List<ImageTextItem> list = null;

                if (list == null || list.Count == 0)
                {
                    list = exchange.GetCoinsListFromPair(selectedPairing);
                }

                listCoins.ItemsSource = null;
                listCoins.ItemsSource = list;
            }
            initSelectionCondition();
        }

        private void listCoins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)e.Source).SelectedItem != null)
            {
                selectedCoin = (Coins)Enum.Parse(typeof(Coins), ((ImageTextItem)((ListBox)e.Source).SelectedItem).text.Content.ToString());
                gridCondition.Visibility = Visibility.Visible;

                /*CellGridAddPopup dialog = new CellGridAddPopup();
                dialog.Exchange = selectedExchange;
                dialog.Coin = selectedCoin;


                dialog.ShowDialog();

                if (dialog.DialogResult.HasValue && dialog.DialogResult == true)
                {
                    Application curApp = Application.Current;
                    MainWindow mainWindow = (MainWindow)curApp.MainWindow;

                    //CellCoin cellCoin = new CellCoin(selectedExchange, selectedPairing, selectedCoin);
                    //cellCoin.cellGrid.addIndicator(dialog.KlinesWidth, dialog.Indicator, dialog.Condition, dialog.ConditionValue);
                    //mainWindow.listCellCoin.Children.Add(cellCoin);

                    AlertLayout alert = new AlertLayout(selectedExchange, selectedCoin, selectedPairing);
                    alert.Exchange = selectedExchange;
                    alert.Coin = selectedCoin;
                    alert.Pair = selectedPairing;
                    AlertCard card = new AlertCard(dialog.KlinesWidth, selectedExchange, selectedCoin, selectedPairing, dialog.Indicator);
                    card.addCondition(dialog.Condition, dialog.ConditionValue);
                    alert.addTo(card);



                    this.Visibility = Visibility.Collapsed;
                    gridCoin.Visibility = Visibility.Collapsed;
                    gridPairing.Visibility = Visibility.Collapsed;
                    gridExchange.Visibility = Visibility.Collapsed;


                    mainWindow.listCellCoin.Children.Add(alert);
                    mainWindow.reSize();
                }*/
                initSelectionCondition();
                //listCoins.UnselectAll();
            }
        }

        private void initSelectionCondition()
        {
            indicatorBox.SelectedIndex = 0;
            conditionBox.SelectedIndex = 0;
            widthBox.SelectedIndex = 0;
            textValue.Value = 0.0d;
            ConditionValue = 0.0d;

            selectedIndicator = Indicators.PRICE;
            selectedCondition = IndicatorConditions.CROSS;
            selectedWidth = CandleWidth.m5;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("DBG-INIT: Sidebar loaded");
        }

        private void indicatorBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();

            Array ar = Enum.GetValues(typeof(Indicators));
            for (int i = 1; i < ar.Length; i++)
            {
                data.Add(((Indicators)ar.GetValue(i)).ToString());
            }

            ((ComboBox)e.Source).ItemsSource = data;
            ((ComboBox)e.Source).SelectedIndex = 0;
        }

        private void conditionBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();

            Array ar = Enum.GetValues(typeof(IndicatorConditions));
            for (int i = 1; i < ar.Length; i++)
            {
                data.Add(((IndicatorConditions)ar.GetValue(i)).ToString());
            }

            ((ComboBox)e.Source).ItemsSource = data;
            ((ComboBox)e.Source).SelectedIndex = 0;
        }

        private void widthBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();

            Array ar = Enum.GetValues(typeof(CandleWidth));
            for (int i = 1; i < ar.Length; i++)
            {
                data.Add(((CandleWidth)ar.GetValue(i)).ToString());
            }

            ((ComboBox)e.Source).ItemsSource = data;
            ((ComboBox)e.Source).SelectedIndex = 0;
        }

        private void indicatorBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedIndicator = (Indicators)Enum.Parse(typeof(Indicators), ((ComboBox)e.Source).SelectedItem.ToString());
        }

        private void conditionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCondition = (IndicatorConditions)Enum.Parse(typeof(IndicatorConditions), ((ComboBox)e.Source).SelectedItem.ToString());
        }

        private void widthBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedWidth = (CandleWidth)Enum.Parse(typeof(CandleWidth), ((ComboBox)e.Source).SelectedItem.ToString());
        }

        private void AddIndicatorBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedExchange != Exchanges.INIT && selectedPairing != Coins.INIT && selectedCoin != Coins.INIT && selectedIndicator != Indicators.INIT
                && selectedCondition != IndicatorConditions.INIT && selectedWidth != CandleWidth.INIT)
            {
                Application curApp = Application.Current;
                MainWindow mainWindow = (MainWindow)curApp.MainWindow;

                var v = ConditionValue;
                mainWindow.addIndicator(selectedExchange, selectedCoin, selectedPairing, selectedWidth, selectedIndicator, selectedCondition, ConditionValue);

                this.Visibility = Visibility.Collapsed;
                gridCondition.Visibility = Visibility.Collapsed;
                gridCoin.Visibility = Visibility.Collapsed;
                gridPairing.Visibility = Visibility.Collapsed;
                gridExchange.Visibility = Visibility.Collapsed;
                mainWindow.darkener.Visibility = Visibility.Collapsed;

            }
        }

        private void textValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ConditionValue = (double)e.NewValue;
        }
    }
}
