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
    /// Interaction logic for CellGrid.xaml
    /// </summary>
    public partial class CellGrid : UserControl
    {
        public CellCoin cellCoin;
        public Exchanges exchanges;
        public Coins coin;

        public CellGrid(CellCoin cellCoin, Exchanges exchanges, Coins coin)
        {
            InitializeComponent();

            this.cellCoin = cellCoin;
            this.exchanges = exchanges;
            this.coin = coin;
        }

        public void addIndicator(CandleStickWidth width, Indicators indicator, IndicatorConditions condition, double value)
        {
            for (int i = 0; i < columnGrid.Children.Count; i++)
            {
                if ((string)((CellColumn)columnGrid.Children[i]).labelCandlestickWidth.Content == App.candleStickWidthToString(width))
                {
                    ((CellColumn)columnGrid.Children[i]).addIndicator(width, indicator, condition, value);
                    setPosition();
                    return;
                }
            }

            CellColumn cellColumn = new CellColumn(this, width, indicator, exchanges, coin);

            cellColumn.addIndicator(width, indicator, condition, value);
            columnGrid.Children.Add(cellColumn);
       
            setPosition();
        }

        public void removeWhenColumnEmpty(CellColumn column)
        {
            columnGrid.Children.Remove(column);
            setPosition();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            setPosition();
        }

        public void setPosition()
        {
            this.UpdateLayout();

            if (columnGrid.Children.Count > 0)
            {
                double widthPerColumn = this.ActualWidth / columnGrid.Children.Count;

                if(widthPerColumn < 200.0d)
                {
                    widthPerColumn = 200;
                }

                IEnumerable<CellColumn> query = columnGrid.Children.Cast<CellColumn>().OrderBy(cc => App.stringToCandleStickWidth((string)cc.labelCandlestickWidth.Content));

                int i = 0;
                double maxColumnHeight = 0;
                foreach(CellColumn cc in query)
                {
                    Canvas.SetLeft(cc, i * widthPerColumn);
                    cc.Width = widthPerColumn;
                    i++;

                    if(cc.ActualHeight > maxColumnHeight)
                    {
                        maxColumnHeight = cc.ActualHeight;
                    }
                }

                cellCoin.Height = cellCoin.header.ActualHeight + maxColumnHeight;

            } else
            {
                cellCoin.Height = cellCoin.header.ActualHeight;
            }
        }
    }
}
