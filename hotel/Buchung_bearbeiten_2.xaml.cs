using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace hotel
{
    public partial class Buchung_bearbeiten_2 : Page
    {
        private int buchungID;
        private string connectionString = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";

        
        public Buchung_bearbeiten_2(int buchungID)
        {
            InitializeComponent();
            this.buchungID = buchungID;

            LadeBuchungsDetails();
            LadeLeistungen();
        }

        private void LadeBuchungsDetails()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                        SELECT b.id AS 'Buchungs-ID', b.datum AS 'Datum', 
                               b.rechnung_id AS 'Rechnungs-ID', b.zimmer_id AS 'Zimmer-ID',
                               k.vorname AS 'Kunden-Vorname', k.nachname AS 'Kunden-Nachname'
                        FROM buchung b
                        INNER JOIN rechnung r ON b.rechnung_id = r.id
                        INNER JOIN kunde k ON r.kunden_id = k.id
                        WHERE b.id = @buchungID;";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@buchungID", buchungID);

                    DataTable dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow row = dataTable.Rows[0];
                        buchungDetailsTextBlock.Text = $"Buchungs-ID: {row["Buchungs-ID"]}\n" +
                                                       $"Datum: {row["Datum"]}\n" +
                                                       $"Rechnungs-ID: {row["Rechnungs-ID"]}\n" +
                                                       $"Zimmer-ID: {row["Zimmer-ID"]}\n" +
                                                       $"Kunde: {row["Kunden-Vorname"]} {row["Kunden-Nachname"]}";
                    }
                    else
                    {
                        buchungDetailsTextBlock.Text = "Keine Daten gefunden.";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Laden der Buchungsdetails: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LadeLeistungen()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                        SELECT l.id, l.leistung, 
                               IF(bl.buchung_id IS NOT NULL, 1, 0) AS gebucht
                        FROM leistungen l
                        LEFT JOIN buchung_hat_leistung bl ON l.id = bl.leistung_id AND bl.buchung_id = @buchungID;";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@buchungID", buchungID);

                    DataTable dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    foreach (DataRow row in dataTable.Rows)
                    {
                        CheckBox checkBox = new CheckBox
                        {
                            Content = row["leistung"].ToString(),
                            Tag = row["id"], 
                            IsChecked = row["gebucht"].ToString() == "1"
                        };
                        leistungenStackPanel.Children.Add(checkBox);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Laden der Zusatzleistungen: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SpeichernButton_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string deleteQuery = "DELETE FROM buchung_hat_leistung WHERE buchung_id = @buchungID;";
                    MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection);
                    deleteCmd.Parameters.AddWithValue("@buchungID", buchungID);
                    deleteCmd.ExecuteNonQuery();

                    // Füge die ausgewählten Leistungen hinzu
                    foreach (CheckBox checkBox in leistungenStackPanel.Children)
                    {
                        if (checkBox.IsChecked == true)
                        {
                            string insertQuery = "INSERT INTO buchung_hat_leistung (buchung_id, leistung_id) VALUES (@buchungID, @leistungID);";
                            MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                            insertCmd.Parameters.AddWithValue("@buchungID", buchungID);
                            insertCmd.Parameters.AddWithValue("@leistungID", checkBox.Tag);
                            insertCmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Änderungen erfolgreich gespeichert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Speichern der Änderungen: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
