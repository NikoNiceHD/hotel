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

            int rechnungsID = ErstelleRechnung(startDatum, endDatum);

            if (rechnungsID > 0)
            {
                Buchung_3 buchung3Page = new Buchung_3(kundenID, startDatum, endDatum, rechnungsID);
                this.NavigationService.Navigate(buchung3Page);
            }
            else
            {
                MessageBox.Show("Fehler beim Erstellen der Rechnung.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int ErstelleRechnung(DateTime startDatum, DateTime endDatum)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string insertRechnungQuery = @"
                        INSERT INTO rechnung (anfang, ende, kunden_id)
                        VALUES (@startDatum, @endDatum, @kundenID);
                        SELECT LAST_INSERT_ID();";

                    MySqlCommand rechnungCmd = new MySqlCommand(insertRechnungQuery, connection);
                    rechnungCmd.Parameters.AddWithValue("@startDatum", startDatum);
                    rechnungCmd.Parameters.AddWithValue("@endDatum", endDatum);
                    rechnungCmd.Parameters.AddWithValue("@kundenID", kundenID);

                    int rechnungsID = Convert.ToInt32(rechnungCmd.ExecuteScalar());
                    return rechnungsID;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Erstellen der Rechnung: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return -1;
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }
    }
}
