﻿using Alerts.Logic.Enums;
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
    /// Interaction logic for ImageTextItem.xaml
    /// </summary>
    public partial class ImageTextItem : UserControl
    {
        public Coins coin { get; set; }

        public ImageTextItem()
        {
            InitializeComponent();

        }
    }
}
