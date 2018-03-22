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
using Alerts.Logic;
using LiveCharts;
using LiveCharts.Wpf;

namespace Alerts.UI.Graphs
{
    /// <summary>
    /// Interaction logic for LineChart.xaml
    /// </summary>
    public partial class LineChart : UserControl, ChartIF
    {
        public SeriesCollection SeriesCollection { get; set; }

        public LineChart()
        {
            InitializeComponent();

            SeriesCollection = new SeriesCollection();

            SeriesCollection.Add(new LineSeries
            {
                Values = new ChartValues<double> { 5, 3, 2, 4 },
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines

            });

            DataContext = this;
        }

        public void AddLineSeries(SeriesCollection SeriesCollection)
        {
            this.SeriesCollection = SeriesCollection;
        }
    }
}
