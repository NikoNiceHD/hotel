using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        // Verbindungszeichenfolge zur MySQL-Datenbank
        private string connectionString = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";

        // Liste für ausgewählte Zimmer
        private List<int> ausgewaehlteZimmer = new List<int>();

        // Konstruktor, der die Kunden-ID, Startdatum und Enddatum empfängt
        public Buchung_3(int kundenID, DateTime startDatum, DateTime endDatum)
        {
            InitializeComponent();
            this.kundenID = kundenID;
            this.startDatum = startDatum;
            this.endDatum = endDatum;

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
                    connection.Open();

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

        // Event-Handler für die Auswahl von Zimmern
        private void ZimmerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Leere die Liste der ausgewählten Zimmer
            ausgewaehlteZimmer.Clear();

            // Füge die ausgewählten Zimmer zur Liste hinzu
            try
            {
                foreach (DataRowView selectedRow in zimmerDataGrid.SelectedItems)
                {
                    int zimmernummer = Convert.ToInt32(selectedRow["Zimmer Nummer"]);
                    ausgewaehlteZimmer.Add(zimmernummer);
                }
            }
            catch
            {
                MessageBox.Show("Es konnte kein Zimmer ausgewählt werden.");
            }
        }

        // Event-Handler für die Buchung der ausgewählten Zimmer
        private void ZimmerAuswaehlen_Click(object sender, RoutedEventArgs e)
        {
            if (ausgewaehlteZimmer.Count > 0)
            {
                // Rechnung erstellen
                int rechnungsID = ErstelleRechnung(startDatum, endDatum);

                if (rechnungsID > 0)
                {
                    // Buchung für jedes ausgewählte Zimmer erstellen
                    foreach (int zimmernummer in ausgewaehlteZimmer)
                    {
                        erstelleBuchung(zimmernummer, rechnungsID);
                    }

                    MessageBox.Show($"{ausgewaehlteZimmer.Count} Zimmer erfolgreich gebucht!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Fehler beim Erstellen der Rechnung.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie mindestens ein Zimmer aus.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Methode zum Erstellen der Rechnung
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

        // Methode zum Erstellen der Buchung für ein einzelnes Zimmer
        private void erstelleBuchung(int zimmernummer, int rechnungsID)
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
                            buchungCmd.Parameters.AddWithValue("@datum", startDatum); // Verwende das Startdatum
                            buchungCmd.Parameters.AddWithValue("@rechnungsID", rechnungsID);
                            buchungCmd.Parameters.AddWithValue("@zimmernummer", zimmernummer);

                            buchungCmd.ExecuteNonQuery();

                            // Transaktion erfolgreich abschließen
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            // Transaktion zurückrollen, falls ein Fehler auftritt
                            transaction.Rollback();
                            MessageBox.Show($"Fehler beim Buchen des Zimmers {zimmernummer}: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler bei der Verbindung zur Datenbank: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}