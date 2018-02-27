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
    /// Interaction logic for CellCoinHeader.xaml
    /// </summary>
    public partial class CellCoinHeader : UserControl
    {
        CellCoin cell;


        public CellCoinHeader()
        {
            InitializeComponent();

        }


        public CellCoinHeader(CellCoin cell)
        {
            InitializeComponent();

            this.cell = cell;
            
        }
    }
}
