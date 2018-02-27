using Alerts.Logic.Enums;
using Alerts.UI.Dialogs;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for CellHeader.xaml
    /// </summary>
    public partial class CellHeader : UserControl
    {
        public CellIndicator cellIndicator;
        public CandleStickWidth candlestickWidth;
        private Indicators _indicator;
        private double _pvalue;
        private double _pvalueChange;
        public Indicators indicator
        {
            get
            {
                return _indicator;
            }
            set
            {
                _indicator = value;
                labelIndicatorName.Content = Enum.GetName(typeof(Indicators), indicator);
            }
        }
        public double PValue
        {
            get
            {
                return _pvalue;
            }
            set
            {
                _pvalue = value;
                labelIndicatorValue.Content = value.ToString("0.00######", CultureInfo.InvariantCulture);
            }
        }
                
        public double PValueChange {
            get
            {
                return _pvalueChange;
            }
            set
            {
                _pvalueChange = value;
                labelIndicatorChange.Content = value.ToString("0.00######", CultureInfo.InvariantCulture);
            }
        }


        public CellHeader()
        {
            InitializeComponent();
        }

        public CellHeader(CellIndicator cellIndicator, CandleStickWidth candlestickWidth, Indicators indicator)
        {
            InitializeComponent();
            this.cellIndicator = cellIndicator;
            this.candlestickWidth = candlestickWidth;
            this.indicator = indicator;
        }

        private void btnAddCondition_Click(object sender, RoutedEventArgs e)
        {
            CellIndicatorAddPopup addPopup = new CellIndicatorAddPopup(this, candlestickWidth, indicator);
            addPopup.ShowDialog();
            if(addPopup.DialogResult.HasValue && addPopup.DialogResult.Value == true)
            {
                cellIndicator.addCondition(addPopup.Condition, addPopup.ConditionValue);
            }
        }

        private void btnMuteAll_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnRemoveAll_Click(object sender, RoutedEventArgs e)
        {
            cellIndicator.removeAll();
        }
    }
}
