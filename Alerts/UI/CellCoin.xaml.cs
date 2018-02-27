using Alerts.Logic.Enums;
using Alerts.UI.Dialogs;
using RestSharp;
using System;
using System.Collections;
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
using TicTacTec.TA.Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alerts.UI
{
    /// <summary>
    /// Interaction logic for CellCoin.xaml
    /// </summary>
    public partial class CellCoin : UserControl
    {
        public CellGrid cellGrid;
        public CellCoinHeader header;
        public Coins coin;
        public Exchanges exchange;

        public CellCoin()
        {
            InitializeComponent();

            header = new CellCoinHeader(this);
            cellGrid = new CellGrid(this, Exchanges.INIT, Coins.INTI);

            Grid.SetRow(header, 0);
            Grid.SetRow(cellGrid, 1);

            header.btnAdd.Click += (o, e) => { addIndicatorDialog(); };

            cellCoinGrid.Children.Add(header);
            cellCoinGrid.Children.Add(cellGrid);
        }

        public CellCoin(Coins coin, Exchanges exchange)
        {
            InitializeComponent();

            this.coin = coin;
            this.exchange = exchange;

            header = new CellCoinHeader(this);
            cellGrid = new CellGrid(this, exchange, coin);

            Grid.SetRow(header, 0);
            Grid.SetRow(cellGrid, 1);

            header.labelCoin.Content = coin.ToString();
            header.labelExchange.Content = exchange.ToString();

            header.btnAdd.Click += (o, e) => { addIndicatorDialog(); };

            cellCoinGrid.Children.Add(header);
            cellCoinGrid.Children.Add(cellGrid);

            header.btnRemove.Click += (o, e) => {
                testrest();
            };

            
        }

        private void testrest()
        {
            int limitkj = 100;

            RestClient client = new RestClient("https://api.binance.com");
            RestRequest request = new RestRequest("api/v1/klines?symbol=BTCUSDT&interval=5m&limit=" + limitkj);

            IRestResponse response = client.Execute(request);

            JArray jArray = JArray.Parse(response.Content);

            double[] close = new double[limitkj];
            for (int i = 0; i < jArray.Count; i++)
            {
                JToken token = jArray[i];

                string closePriceAsString = token[4].ToString();
                double closePrice = Double.Parse(closePriceAsString);
                close[i] = closePrice;
            }

            int a;
            int b;
            double[] c = new double[limitkj];
            TicTacTec.TA.Library.Core.Rsi(0, limitkj - 1, close, 14, out a, out b, c);

            for (int i = 0; i < c.Length; i++)
            {
                System.Diagnostics.Debug.WriteLine("DBG-REST: " + c[i] + " " + a + " " + b);
            }


            /*double[] ar = { 4086.29, 4310.01, 4509.08, 4130.37, 3699.99, 3660.02, 4378.48, 4640.00, 5709.99, 5950.02, 6169.98, 7345.01, 5811.03, 8038.00, 9128.02, 11165.41 };

            int a;
            int b;
            double[] c = new double[16];

            TicTacTec.TA.Library.Core.Rsi(0, 15, ar, 14, out a, out b, c);

            /*
            [
                [
                    1499040000000,      // Open time
                    "0.01634790",       // Open
                    "0.80000000",       // High
                    "0.01575800",       // Low
                    "0.01577100",       // Close
                    "148976.11427815",  // Volume
                    1499644799999,      // Close time
                    "2434.19055334",    // Quote asset volume
                    308,                // Number of trades
                    "1756.87402397",    // Taker buy base asset volume
                    "28.46694368",      // Taker buy quote asset volume
                    "17928899.62484339" // Ignore
                ]
            ]
             */
        }

        public void addIndicatorDialog()
        {
            CellGridAddPopup dialog = new CellGridAddPopup();
            dialog.ShowDialog();

            if(dialog.DialogResult.HasValue && dialog.DialogResult == true)
            {
                cellGrid.addIndicator(dialog.KlinesWidth, dialog.Indicator, dialog.Condition, dialog.ConditionValue);
            }

            //Application curApp = Application.Current;
            //MainWindow mainWindow = (MainWindow)curApp.MainWindow;
        }
    }
}
