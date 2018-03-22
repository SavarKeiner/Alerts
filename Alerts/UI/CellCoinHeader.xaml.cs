using Alerts.Logic.Events;
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
using Alerts.Logic.Enums;

namespace Alerts.UI
{
    /// <summary>
    /// Interaction logic for CellCoinHeader.xaml
    /// </summary>
    public partial class CellCoinHeader : UserControl
    {
        private Exchanges exchange;
        private string symbol;
        private double price;
        public Exchanges Exchange
        {
            get
            {
                return exchange;
            }
            set
            {
                exchange = value;
                labelExchange.Content = value.ToString().ToUpper();
            }
        }
        public string Symbol
        {
            get
            {
                return symbol;
            }
            set
            {
                symbol = value;
                labelCoin.Content = value;
            }
        }
        public double Price
        {
            get
            {
                return price;
            }
            set
            {
                price = value;
                labelPrice.Content = value.ToString("0.00000000");
            }
        }

        public CellCoinHeader()
        {
            InitializeComponent();

        }

        public void initPull(Object o, CandlePullEventArgs e)
        {
            if (e.Width == Logic.Enums.CandleWidth.INIT)
            {
                this.Dispatcher.Invoke(() => {
                    labelPrice.Content = e.candleList[e.candleList.Count - 1].getClose().ToString("0.00000000");
                });
            }
        }
    }
}
