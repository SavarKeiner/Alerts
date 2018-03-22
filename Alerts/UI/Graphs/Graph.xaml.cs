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
using Alerts.Logic;
using Alerts.Logic.Enums;

namespace Alerts.UI.Graphs
{
    /// <summary>
    /// Interaction logic for Graph.xaml
    /// </summary>
    public partial class Graph : UserControl
    {
        public Indicators Indicator { get; set; }

        private ChartIF chart;

        public Graph(Indicators Indicator)
        {
            switch (Indicator)
            {
                case Indicators.RSI:
                    LineChart lineChart = new LineChart();
                    chart = lineChart;
                    DataContext = lineChart;
                    break;
                
            }

            InitializeComponent();
        }
    }
}
