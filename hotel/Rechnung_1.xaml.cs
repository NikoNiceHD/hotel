using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace hotel
{
    public partial class Rechnung_1 : Page
    {
        private string connectionString = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";
        private DataView kundenDataView; // Für Filterung

        public Rechnung_1()
        {
            InitializeComponent();
            LadeKundenUndRechnungen();
        }

        private void LadeKundenUndRechnungen()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                SELECT 
    kunde.vorname AS 'Kunden-Vorname', 
    kunde.nachname AS 'Kunden-Nachname', 
    rechnung.id AS 'Rechnungs-ID', 
    COALESCE(
        (SELECT SUM(leistungen.preis * (DATEDIFF(rechnung.ende, rechnung.anfang) + 1)) 
         FROM buchung_hat_leistung 
         JOIN leistungen ON buchung_hat_leistung.leistung_id = leistungen.id
         WHERE buchung_hat_leistung.buchung_id IN 
               (SELECT buchung.id FROM buchung WHERE buchung.rechnung_id = rechnung.id)
        ), 0)
    +
    COALESCE(
        (SELECT SUM(eigenschaften.preis * (DATEDIFF(rechnung.ende, rechnung.anfang) + 1)) 
         FROM zimmer_hat_eigenschaften 
         JOIN eigenschaften ON zimmer_hat_eigenschaften.eigenschaften_id = eigenschaften.id
         WHERE zimmer_hat_eigenschaften.zimmer_id IN 
               (SELECT buchung.zimmer_id FROM buchung WHERE buchung.rechnung_id = rechnung.id)
        ), 0) 
    AS 'Gesamtpreis'
FROM rechnung
JOIN kunde ON rechnung.kunden_id = kunde.id
GROUP BY rechnung.id, kunde.vorname, kunde.nachname;
";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    DataTable dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    kundenDataView = dataTable.DefaultView;
                    KundenDataGrid.ItemsSource = kundenDataView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler: " + ex.Message);
                }
            }
        }

        private void SucheTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (kundenDataView != null)
            {
                // Filtert die DataView basierend auf der Eingabe
                string filterText = sucheTextBox.Text.Trim();
                if (string.IsNullOrEmpty(filterText))
                {
                    kundenDataView.RowFilter = ""; // Kein Filter
                }
                else
                {
                    // Filtere nach Vorname, Nachname oder Rechnungs-ID
                    kundenDataView.RowFilter = $@"
                [Kunden-Vorname] LIKE '%{filterText}%' OR
                [Kunden-Nachname] LIKE '%{filterText}%' OR
                CONVERT([Rechnungs-ID], 'System.String') LIKE '%{filterText}%'";
                }
            }
        }

        // Event-Handler für Doppelklick im DataGrid
        private void KundenDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (KundenDataGrid.SelectedItem != null)
            {
                // Ermittle die ausgewählte Rechnungs-ID
                DataRowView selectedRow = KundenDataGrid.SelectedItem as DataRowView;
                if (selectedRow != null)
                {
                    int rechnungsId = Convert.ToInt32(selectedRow["Rechnungs-ID"]);

                    // Weiterleitung zur Rechnung_2-Seite mit der Rechnungs-ID
                    Rechnung_2 rechnung2Page = new Rechnung_2(rechnungsId);
                    this.NavigationService.Navigate(rechnung2Page);
                }
            }
        }
    }
}