using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace hotel
{
    public partial class Buchung_3 : Page
    {
        private int kundenID;
        private DateTime startDatum;
        private DateTime endDatum;
        private int rechnungsID; // Rechnungs-ID, die von Buchung_2 übergeben wird

        // Verbindungszeichenfolge zur MySQL-Datenbank
        private string connectionString = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";

        // Konstruktor, der die Kunden-ID, Startdatum, Enddatum und Rechnungs-ID empfängt
        public Buchung_3(int kundenID, DateTime startDatum, DateTime endDatum, int rechnungsID)
        {
            InitializeComponent();
            this.kundenID = kundenID;
            this.startDatum = startDatum;
            this.endDatum = endDatum;
            this.rechnungsID = rechnungsID;

            // Zeige die übergebenen Daten an
            kundenIDTextBlock.Text = $"Kunden-ID: {kundenID}";
            startDatumTextBlock.Text = $"Startdatum: {startDatum.ToShortDateString()}";
            endDatumTextBlock.Text = $"Enddatum: {endDatum.ToShortDateString()}";

            // Lade verfügbare Zimmer
            LadeVerfuegbareZimmer();
        }

        // Methode zum Laden der verfügbaren Zimmer
        private void LadeVerfuegbareZimmer()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open(); // Verbindung hier öffnen

                    string query = @"
                        SELECT DISTINCT buchung.zimmer_id AS 'Zimmer Nummer', gebaeude.id AS 'Gebäude Nummer', adresse.strasse, adresse.plz
                        FROM buchung
                        INNER JOIN zimmer ON buchung.zimmer_id = zimmer.id
                        INNER JOIN gebaeude ON zimmer.gebaeude_id = gebaeude.id
                        INNER JOIN adresse ON gebaeude.adress_id = adresse.id
                        WHERE zimmer_id NOT IN 
                        (
                            SELECT DISTINCT buchung.zimmer_id
                            FROM buchung
                            WHERE buchung.datum BETWEEN @startDatum AND @endDatum
                        );";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@startDatum", startDatum);
                    cmd.Parameters.AddWithValue("@endDatum", endDatum);

                    DataTable dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());
                    zimmerDataGrid.ItemsSource = dataTable.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Laden der Zimmer: " + ex.Message);
                }
                finally
                {
                    // Verbindung schließen, falls sie geöffnet ist
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }

        // Event-Handler für die Auswahl eines Zimmers
        private void ZimmerAuswaehlen_Click(object sender, RoutedEventArgs e)
        {
            if (zimmerDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = zimmerDataGrid.SelectedItem as DataRowView;
                int zimmernummer = Convert.ToInt32(selectedRow["Zimmer Nummer"]);

                // Buchung erstellen
                erstelleBuchung(zimmernummer);
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie ein Zimmer aus.");
            }
        }

        // Methode zum Erstellen der Buchung
        private void erstelleBuchung(int zimmernummer)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Transaktion starten
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Buchung in die Tabelle `buchung` einfügen
                            string insertBuchungQuery = @"
                                INSERT INTO buchung (datum, rechnung_id, zimmer_id)
                                VALUES (@datum, @rechnungsID, @zimmernummer);";

                            MySqlCommand buchungCmd = new MySqlCommand(insertBuchungQuery, connection, transaction);
                            buchungCmd.Parameters.AddWithValue("@datum", DateTime.Now); // Aktuelles Datum für die Buchung
                            
                            buchungCmd.Parameters.AddWithValue("@rechnungsID", rechnungsID);
                            buchungCmd.Parameters.AddWithValue("@zimmernummer", zimmernummer);

                            buchungCmd.ExecuteNonQuery();

                            // Transaktion erfolgreich abschließen
                            transaction.Commit();

                            MessageBox.Show("Buchung erfolgreich erstellt!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            // Transaktion zurückrollen, falls ein Fehler auftritt
                            transaction.Rollback();
                            MessageBox.Show("Fehler beim Erstellen der Buchung: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler bei der Verbindung zur Datenbank: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ZimmerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: Hier können Sie zusätzliche Logik bei der Auswahl eines Zimmers implementieren
        }
    }
}