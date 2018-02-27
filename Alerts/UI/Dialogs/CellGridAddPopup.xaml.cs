using Alerts.Logic.Enums;
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
using System.Windows.Shapes;

namespace Alerts.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for CellGridAddPopup.xaml
    /// </summary>
    public partial class CellGridAddPopup : Window
    {
        public Indicators Indicator{ get; set; }
        public CandleStickWidth KlinesWidth { get; set; }
        public IndicatorConditions Condition { get; set; }
        public Exchanges Exchange { get; set; }
        public Coins Coin { get; set; }
        public double ConditionValue { get; set; }

        public CellGridAddPopup()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Application curApp = Application.Current;
            Window mainWindow = curApp.MainWindow;
            this.Left = mainWindow.Left + (mainWindow.Width - this.ActualWidth) / 2;
            this.Top = mainWindow.Top + (mainWindow.Height - this.ActualHeight) / 2;

            textValue.Text = "0.0";
        }

        private void loadConditions(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();

            Array ar = Enum.GetValues(typeof(IndicatorConditions));
            for (int i = 1; i < ar.Length; i++)
            {
                data.Add(((IndicatorConditions)ar.GetValue(i)).ToString());
            }

            conditionSelect.ItemsSource = data;
            conditionSelect.SelectedIndex = 0;
        }

        private void loadCandlestickWidth(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();

            Array ar = Enum.GetValues(typeof(CandleStickWidth));
            for (int i = 1; i < ar.Length; i++)
            {
                data.Add(App.candleStickWidthToString((CandleStickWidth)ar.GetValue(i)));
            }

            ((ComboBox)e.Source).ItemsSource = data;
            ((ComboBox)e.Source).SelectedIndex = 0;
        }

        private void click_confirm(object sender, RoutedEventArgs e)
        {
            if(Indicator != Indicators.INIT)
            {
                DialogResult = true;
                Close();
            }
        }

        private void btnVolumeClick(object sender, RoutedEventArgs e)
        {
            Indicator = Indicators.VOLUME;
        }

        private void btnPriceClick(object sender, RoutedEventArgs e)
        {
            Indicator = Indicators.PRICE;
        }

        private void btnRsiClick(object sender, RoutedEventArgs e)
        {
            Indicator = Indicators.RSI;
        }

        private void conditionSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Condition = (IndicatorConditions)Enum.Parse(typeof(IndicatorConditions), ((ComboBox)e.Source).SelectedItem.ToString());
        }

        private void candlestickWidth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KlinesWidth = (CandleStickWidth)Enum.Parse(typeof(CandleStickWidth), App.stringToCandleStickWidth(((ComboBox)e.Source).SelectedItem.ToString()).ToString());
        }

        private void textValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ConditionValue = (double)e.NewValue;
        }


    }
}
