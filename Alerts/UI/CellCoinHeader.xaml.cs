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
        private Exchanges _exchange;
        private string _symbol;
        private double _price;
        public Exchanges Exchange
        {
            get
            {
                return _exchange;
            }
            set
            {
                _exchange = value;
                labelExchange.Content = value.ToString().ToUpper();
            }
        }
        public string Symbol
        {
            get
            {
                return _symbol;
            }
            set
            {
                _symbol = value;
                labelCoin.Content = value;
            }
        }
        public double Price
        {
            get
            {
                return _price;
            }
            set
            {
                _price = value;
                labelPrice.Content = value.ToString("0.00000000");
            }
        }
        public AlertLayout alert { get; set; }

        private Indicators selectedIndicator { get; set; }
        private IndicatorConditions selectedCondition { get; set; }
        private CandleWidth selectedWidth { get; set; }
        private double ConditionValue { get; set; } = 0;

        public CellCoinHeader()
        {
            InitializeComponent();

            AddIndicatorL.Visibility = Visibility.Collapsed;
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

        private void indicatorBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedIndicator = (Indicators)Enum.Parse(typeof(Indicators), ((ComboBox)e.Source).SelectedItem.ToString());
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

        private void conditionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCondition = (IndicatorConditions)Enum.Parse(typeof(IndicatorConditions), ((ComboBox)e.Source).SelectedItem.ToString());
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

        private void widthBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedWidth = (CandleWidth)Enum.Parse(typeof(CandleWidth), ((ComboBox)e.Source).SelectedItem.ToString());
        }

        private void textValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ConditionValue = (double)e.NewValue;
        }

        private void AddIndicatorBtn_Click(object sender, RoutedEventArgs e)
        {
            alert.add( selectedWidth, selectedIndicator, selectedCondition, ConditionValue);
            AddIndicatorL.Visibility = Visibility.Collapsed;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            initSelection();
            AddIndicatorL.Visibility = Visibility.Visible;
        }

        private void initSelection()
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            AddIndicatorL.Visibility = Visibility.Collapsed;
        }
    }
}
