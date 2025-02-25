﻿using System;
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

namespace hotel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_kunden(object sender, RoutedEventArgs e)
        {
            main.Content = new Kunden();
        }

        private void btn_zimmer(object sender, RoutedEventArgs e)
        {
            main.Content = new Zimmer();
        }

        private void btn_buchung(object sender, RoutedEventArgs e)
        {
            main.Content = new Buchung_1();
        }

        private void btn_rechnung(object sender, RoutedEventArgs e)
        {
            main.Content = new Rechnung_1();
        }
    }
}