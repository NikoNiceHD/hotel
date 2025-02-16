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
    /// Interaktionslogik für Buchung_1.xaml
    /// </summary>
    public partial class Buchung_1 : Page
    {
        public Buchung_1()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Buchung.Navigate(new Buchung());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Buchung.Navigate(new Buchung_bearbeiten());
        }
    }
}
