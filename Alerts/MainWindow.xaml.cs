﻿using Alerts.UI;
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

namespace Alerts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            sideBar.Visibility = Visibility.Collapsed;
            sideBar.gridExchange.Visibility = Visibility.Collapsed;
            sideBar.gridPairing.Visibility = Visibility.Collapsed;
            sideBar.gridCoin.Visibility = Visibility.Collapsed;
        }

        public void addCellCoin()
        {

        }

        public void removeCellCoin()
        {

        }

        public void reSize()
        {
            CellCoin lastCellCoin = null;
            double lastY = 0;
            foreach (CellCoin cc in listCellCoin.Children)
            {
                if(lastCellCoin == null)
                {
                    //Canvas.SetTop(cc, 0);
                    //Canvas.SetLeft(cc, 0);
                } else
                {
                    Canvas.SetTop(cc, lastY);
                    //Canvas.SetLeft(cc, 0);
                }
                

                cc.Width = listCellCoin.Width;
                lastCellCoin = cc;
                lastY += cc.ActualHeight;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("shit: " + e.NewSize.Width + " " + sideMenu.ActualWidth);

            listCellCoin.Width = e.NewSize.Width - sideMenu.ActualWidth - 16;
            reSize();
        }
    }
}
