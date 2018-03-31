using Alerts.Logic.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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
                labelConditionValue.Content = value.ToString("0.00######");
            }
        }

        public long lastSeenOpenTime { get; set; } = 0;

        public CellCondition(AlertCard card, IndicatorConditions indicatorCondition, double value)
        {
            InitializeComponent();
            btnl.Visibility = Visibility.Collapsed;
            this.indicatorCondition = indicatorCondition;
            this.value = value;
            this.card = card;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            
            //cellIndicator.removeCondition(this);
            card.listCondition.Children.Remove(this);
        }

        private void DockPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            btnl.Visibility = Visibility.Visible;
        }

        private void DockPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            btnl.Visibility = Visibility.Collapsed;
        }

        public void showNotification(string str)
        {

            labelCondition.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));
            labelConditionValue.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));

            Application curApp = Application.Current;
            MainWindow mainWindow = (MainWindow)curApp.MainWindow;

            NotificationsPop pop = new NotificationsPop();
            pop.messageLabel.Content = str;
            pop.closeButton.Click += (o, e) =>
            {
                mainWindow.notificationArea.Children.Remove(pop);
                labelCondition.Foreground = new SolidColorBrush(Colors.White);
                labelConditionValue.Foreground = new SolidColorBrush(Colors.White);
            };

            mainWindow.notificationArea.Children.Add(pop);

            mainWindow.notifySideBar.AddNotification(str + " "  + DateTime.Now.ToString("HH:mm:ss"));

            SystemSounds.Beep.Play();
        }
    }
}
