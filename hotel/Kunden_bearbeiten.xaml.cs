using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace hotel
{
    public partial class Kunden_bearbeiten : Page
    {
        string connectstring = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";
        private DataTable dataTable;
        private string baseQuery = @"
            SELECT kunde.id, kunde.vorname, kunde.nachname, kunde.geburtsdatum, 
                   adresse.strasse, adresse.plz, kunde_hat_adresse.datum AS Einzugsdatum, 
                   plz.ort 
            FROM kunde 
            INNER JOIN kunde_hat_adresse ON kunde_hat_adresse.kunden_id = kunde.id 
            INNER JOIN adresse ON kunde_hat_adresse.adress_id = adresse.id 
            INNER JOIN plz ON plz.plz = adresse.plz";

        public Kunden_bearbeiten()
        {
            InitializeComponent();
            LoadData(baseQuery); // Load all customers initially
        }

        private void LoadData(string query)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectstring))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());
                    dtgrid.DataContext = dataTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Daten: " + ex.Message);
            }
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = searchTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(search))
            {
                string query = $@"{baseQuery} 
                                WHERE kunde.vorname LIKE @search 
                                OR kunde.nachname LIKE @search";

                try
                {
                    using (MySqlConnection connection = new MySqlConnection(connectstring))
                    {
                        connection.Open();
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@search", $"%{search}%");

                        dataTable = new DataTable();
                        dataTable.Load(cmd.ExecuteReader());
                        dtgrid.DataContext = dataTable;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler bei der Suche: " + ex.Message);
                }
            }
            else
            {
                LoadData(baseQuery); // Load all data when search is empty
            }
        }

        // Methode zum Speichern der Änderungen in die Datenbank mit Transaktion
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Bestätigungsdialog anzeigen
            MessageBoxResult result = MessageBox.Show("Möchten Sie die Änderungen speichern?", "Bestätigung", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connectstring))
                {
                    conn.Open();

                    // Beginne eine Transaktion
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Durchlaufe alle geänderten Zeilen im DataTable
                            foreach (DataRow row in dataTable.Rows)
                            {
                                if (row.RowState == DataRowState.Modified) // Nur geänderte Zeilen berücksichtigen
                                {
                                    // Kundendaten aktualisieren
                                    string updateKundeQuery = @"
                                        UPDATE kunde 
                                        SET vorname = @vorname, 
                                            nachname = @nachname, 
                                            geburtsdatum = @geburtsdatum 
                                        WHERE id = @id";

                                    using (MySqlCommand cmd = new MySqlCommand(updateKundeQuery, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@vorname", row["vorname"]);
                                        cmd.Parameters.AddWithValue("@nachname", row["nachname"]);
                                        cmd.Parameters.AddWithValue("@geburtsdatum", row["geburtsdatum"]);
                                        cmd.Parameters.AddWithValue("@id", row["id"]);
                                        cmd.ExecuteNonQuery();
                                    }

                                    // Adressdaten aktualisieren
                                    string updateAdresseQuery = @"
                                        UPDATE adresse 
                                        SET strasse = @strasse, 
                                            plz = @plz 
                                        WHERE id = (SELECT adress_id FROM kunde_hat_adresse WHERE kunden_id = @id)";

                                    using (MySqlCommand cmd = new MySqlCommand(updateAdresseQuery, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@strasse", row["strasse"]);
                                        cmd.Parameters.AddWithValue("@plz", row["plz"]);
                                        cmd.Parameters.AddWithValue("@id", row["id"]);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            // Transaktion bestätigen
                            transaction.Commit();
                            MessageBox.Show("Änderungen erfolgreich gespeichert!");
                        }
                        catch (Exception ex)
                        {
                            // Transaktion rückgängig machen
                            transaction.Rollback();
                            MessageBox.Show("Fehler beim Speichern der Daten: " + ex.Message);
                        }
                    }
                }
            }
        }
    }
}