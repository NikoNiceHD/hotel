using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySqlConnector;

namespace hotel
{
    public partial class Kunden_bearbeiten : Page
    {
        string connectstring = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";
        private DataTable dataTable; // Datenquelle für das DataGrid

        public Kunden_bearbeiten()
        {
            InitializeComponent();
            LoadData("Select kunde.id, kunde.vorname, kunde.nachname, kunde.geburtsdatum, adresse.strasse, adresse.plz, kunde_hat_adresse.datum AS Einzugsdatum, plz.ort FROM kunde " +
                "INNER JOIN kunde_hat_adresse ON kunde_hat_adresse.kunden_id = kunde.id " +
                "INNER JOIN adresse ON kunde_hat_adresse.adress_id = adresse.id " +
                "INNER JOIN plz ON plz.plz = adresse.plz; " 
              // Lade alle Kunden beim Start
            ); // Lade alle Kunden beim Start
        }

        // Methode zum Laden der Daten in das DataGrid
        private void LoadData(string query)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectstring))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());
                    dtgrid.DataContext = dataTable; // Binde die Daten an das DataGrid
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Daten: " + ex.Message);
            }
        }

        // Suchfunktion bei Drücken der Enter-Taste  WHERE vorname LIKE '%{search}%' OR nachname LIKE '%{search}%'
        private void SearchTextBox(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string search = searchTextBox.Text.Trim();

                if (!string.IsNullOrEmpty(search))
                {
                    string query = @"SELECT kunde.id, kunde.vorname, kunde.nachname, kunde.geburtsdatum, adresse.strasse, adresse.plz, 
                        kunde_hat_adresse.datum AS Einzugsdatum, plz.ort 
                 FROM kunde 
                 INNER JOIN kunde_hat_adresse ON kunde_hat_adresse.kunden_id = kunde.id 
                 INNER JOIN adresse ON kunde_hat_adresse.adress_id = adresse.id
                 INNER JOIN plz ON plz.plz = adresse.plz
                 WHERE kunde.vorname LIKE '%" + search + "%' OR kunde.nachname LIKE '%" + search + "%';";

                    LoadData(query);
                }
                else
                {
                    MessageBox.Show("Bitte geben Sie einen Suchbegriff ein.");
                    LoadData(@"SELECT kunde.id, kunde.vorname, kunde.nachname, kunde.geburtsdatum, adresse.strasse, adresse.plz, 
                        kunde_hat_adresse.datum AS Einzugsdatum, plz.ort 
                 FROM kunde 
                 INNER JOIN kunde_hat_adresse ON kunde_hat_adresse.kunden_id = kunde.id 
                 INNER JOIN adresse ON kunde_hat_adresse.adress_id = adresse.id
                 INNER JOIN plz ON plz.plz = adresse.plz;"); // Lade alle Daten, wenn die Suche leer ist
                }
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
                                    string updateQuery = @"
                                        UPDATE azanik.kunde 
                                        SET vorname = @vorname, 
                                            nachname = @nachname, 
                                            geburtsdatum = @geburtsdatum 
                                        WHERE id = @id";

                                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn, transaction))
                                    {
                                        // Parameter hinzufügen
                                        cmd.Parameters.AddWithValue("@vorname", row["vorname"]);
                                        cmd.Parameters.AddWithValue("@nachname", row["nachname"]);
                                        cmd.Parameters.AddWithValue("@geburtsdatum", row["geburtsdatum"]);
                                        cmd.Parameters.AddWithValue("@id", row["id"]);

                                        // SQL-Befehl ausführen
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