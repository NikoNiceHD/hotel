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

namespace hotel
{
    /// <summary>
    /// Interaktionslogik für Kunden.xaml
    /// </summary>
    public partial class Kunden : Page
    {
        public Kunden()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            kunden.Navigate(new Kunden_eintragen());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            kunden.Content = new Kunden_bearbeiten();
        }
    }
}
