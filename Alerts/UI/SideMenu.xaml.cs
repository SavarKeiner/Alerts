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
    /// Interaction logic for SideMenu.xaml
    /// </summary>
    public partial class SideMenu : UserControl
    {
        public SideMenu()
        {
            InitializeComponent();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Grid)((Button)sender).Content).Children[0].Visibility = Visibility.Visible;
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Grid)((Button)sender).Content).Children[0].Visibility = Visibility.Hidden;
        }

        private void addCoinClick(object sender, RoutedEventArgs e)
        {
            Application curApp = Application.Current;
            MainWindow mainWindow = (MainWindow)curApp.MainWindow;

            if (mainWindow.sideBar.Visibility == Visibility.Collapsed)
            {
                mainWindow.sideBar.Visibility = Visibility.Visible;
                mainWindow.sideBar.gridExchange.Visibility = Visibility.Visible;
            }
            else if(mainWindow.sideBar.Visibility == Visibility.Visible)
            {
                mainWindow.sideBar.Visibility = Visibility.Collapsed;
                mainWindow.sideBar.gridCondition.Visibility = Visibility.Collapsed;
                mainWindow.sideBar.gridCoin.Visibility = Visibility.Collapsed;
                mainWindow.sideBar.gridPairing.Visibility = Visibility.Collapsed;
                mainWindow.sideBar.gridExchange.Visibility = Visibility.Collapsed;
            }
        }
    }
}
