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
    /// Interaction logic for MenuBar.xaml
    /// </summary>
    public partial class MenuBar : UserControl
    {
        public MenuBar()
        {
            InitializeComponent();
        }

        private void addAlertClick(object sender, RoutedEventArgs e)
        {
            Application curApp = Application.Current;
            MainWindow mainWindow = (MainWindow)curApp.MainWindow;

            if (mainWindow.sideBar.Visibility == Visibility.Collapsed)
            {
                mainWindow.sideBar.Visibility = Visibility.Visible;
            } else if (mainWindow.sideBar.Visibility == Visibility.Visible)
            {
                mainWindow.sideBar.Visibility = Visibility.Collapsed;
            }
        }
    }
}
