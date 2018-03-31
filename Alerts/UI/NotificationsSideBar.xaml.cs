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
    /// Interaction logic for NotificationsSideBar.xaml
    /// </summary>
    public partial class NotificationsSideBar : UserControl
    {
        public NotificationsSideBar()
        {
            InitializeComponent();

        }

        public void AddNotification(string str)
        {
            //new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));
            NotificationsEntry entry = new NotificationsEntry();
            entry.messageLabel.Content = str;
            notificationsList.Children.Add(entry);
        }
    }
}
