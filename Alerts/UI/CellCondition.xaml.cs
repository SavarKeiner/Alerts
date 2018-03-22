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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Alerts.UI
{
    /// <summary>
    /// Interaction logic for CellCondition.xaml
    /// </summary>
    public partial class CellCondition : UserControl
    {
        public CellIndicator cellIndicator;
        public AlertCard card;
        public CandleWidth candlestickWidth { get; set; }

        private IndicatorConditions _indicatorCondition;
        public IndicatorConditions indicatorCondition {
            get { return _indicatorCondition; }
            set
            {
                _indicatorCondition = value;
                labelCondition.Content = Enum.GetName(typeof(IndicatorConditions), indicatorCondition);
            }
        }

        private double _value;
        public double value
        {
            get { return _value; }
            set
            {
                _value = value;
                labelConditionValue.Content = value;
            }
        }

        public CellCondition()
        {
            InitializeComponent();
        }

        public CellCondition(AlertCard card, IndicatorConditions indicatorCondition, double value)
        {
            InitializeComponent();
            this.indicatorCondition = indicatorCondition;
            this.value = value;
            this.card = card;
        }

        public CellCondition(CellIndicator cellIndicator, CandleWidth candlestickWidth, IndicatorConditions indicatorCondition, double value)
        {
            InitializeComponent();
            this.cellIndicator = cellIndicator;
            this.candlestickWidth = candlestickWidth;
            this.indicatorCondition = indicatorCondition;
            this.value = value;

        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            
            //cellIndicator.removeCondition(this);
            card.listCondition.Children.Remove(this);
        }

        private void btnMute_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
