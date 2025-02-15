using System;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace hotel
{
    public partial class Buchung_erstellen : Page
    {
        private int kundenID;
        private string connectionString = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";

        public Buchung_erstellen(int kundenID)
        {
            InitializeComponent();
            this.kundenID = kundenID;

            textbox_kundenid.Text = $"Kunden-ID: {kundenID}";
            datepicker_start.SelectedDateChanged += Datepicker_start_SelectedDateChanged;
        }

        private void Datepicker_start_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (datepicker_start.SelectedDate.HasValue)
            {
                datepicker_ende.DisplayDateStart = datepicker_start.SelectedDate;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDatum = datepicker_start.SelectedDate ?? DateTime.MinValue;
            DateTime endDatum = datepicker_ende.SelectedDate ?? DateTime.MinValue;

            if (startDatum == DateTime.MinValue || endDatum == DateTime.MinValue)
            {
                MessageBox.Show("Bitte wählen Sie ein gültiges Start- und Enddatum aus.");
                return;
            }

            if (endDatum < startDatum)
            {
                MessageBox.Show("Das Enddatum darf nicht vor dem Startdatum liegen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Navigiere zur Buchung_3-Seite
            Buchung_3 buchung3Page = new Buchung_3(kundenID, startDatum, endDatum);
            this.NavigationService.Navigate(buchung3Page);
        }

      

    }
}