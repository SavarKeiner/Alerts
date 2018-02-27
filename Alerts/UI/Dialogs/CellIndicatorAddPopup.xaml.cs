using Alerts.Logic.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for CellIndicatorAddPopup.xaml
    /// </summary>
    public partial class CellIndicatorAddPopup : Window
    {
        public CandleStickWidth KlinesWidth { get; set; }
        public CandleStickWidth KlinesChange { get; set; }
        public Indicators Indicator { get; set; }
        public IndicatorConditions Condition { get; set; }
        public double ConditionValue { get; set; }

        public CellIndicatorAddPopup()
        {
            InitializeComponent();
        }

        public CellIndicatorAddPopup(CellHeader header, CandleStickWidth candlestickWidth, Indicators indicator)
        {
            InitializeComponent();
            this.KlinesWidth = candlestickWidth;
            this.Indicator = indicator;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Application curApp = Application.Current;
            Window mainWindow = curApp.MainWindow;
            this.Left = mainWindow.Left + (mainWindow.Width - this.ActualWidth) / 2;
            this.Top = mainWindow.Top + (mainWindow.Height - this.ActualHeight) / 2;
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

        private void click_btnConfirm(object sender, RoutedEventArgs e)
        {
            if(textValue.Text != null && textValue.Text.Length > 0)
            {
                DialogResult = true;
                Close();
            }
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

        private void conditionSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Condition = (IndicatorConditions)Enum.Parse(typeof(IndicatorConditions), ((ComboBox)e.Source).SelectedItem.ToString());
        }

        private void klineChagneSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KlinesChange = (CandleStickWidth)Enum.Parse(typeof(CandleStickWidth), App.stringToCandleStickWidth(((ComboBox)e.Source).SelectedItem.ToString()).ToString());
        }

        private void textValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ConditionValue = (double)e.NewValue;
        }
    }
}
