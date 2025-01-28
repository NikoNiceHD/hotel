using System;
using System.Collections.Generic;
using MySqlConnector; // MySQL Connector
using System.Windows;
using System.Windows.Controls;

namespace hotel
{
    public partial class Zimmer_eintragen : Page
    {
        // Verbindungszeichenfolge zur MySQL-Datenbank
        private string connectionString = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";

        public Zimmer_eintragen()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Verbindung zur MySQL-Datenbank herstellen
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
                            // Werte aus den TextBoxen auslesen
                            int zimmerId = int.Parse(TextboxzimmerID.Text);
                            int gebaeudeId = int.Parse(TextboxgebaeudeID.Text);

                            // Überprüfen, ob das Zimmer bereits existiert
                            string checkZimmerQuery = "SELECT COUNT(*) FROM zimmer WHERE id = @zimmerId";
                            using (MySqlCommand checkCommand = new MySqlCommand(checkZimmerQuery, connection, transaction))
                            {
                                checkCommand.Parameters.AddWithValue("@zimmerId", zimmerId);
                                int zimmerCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                                if (zimmerCount > 0)
                                {
                                    MessageBox.Show("Das Zimmer existiert bereits in der Datenbank.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return; // Vorgang abbrechen
                                }
                            }

                            // Überprüfen der CheckBox-Kombinationen
                            if (CheckBoxSuite.IsChecked == true && CheckBoxDoppelzimmer.IsChecked == true)
                            {
                                MessageBox.Show("Ein Zimmer kann nicht gleichzeitig eine Suite und ein Doppelzimmer sein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            if (CheckBoxGrosserBalkon.IsChecked == true && CheckBoxKleinerBalkon.IsChecked == true)
                            {
                                MessageBox.Show("Ein Zimmer kann nicht gleichzeitig einen großen und einen kleinen Balkon haben.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            if ((CheckBoxTerasse.IsChecked == true) && (CheckBoxGrosserBalkon.IsChecked == true || CheckBoxKleinerBalkon.IsChecked == true))
                            {
                                MessageBox.Show("Ein Zimmer kann nicht gleichzeitig eine Terrasse und einen Balkon haben.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // Liste für ausgewählte Eigenschaften erstellen
                            List<int> eigenschaftenIds = new List<int>();

                            // Überprüfen, welche CheckBoxen ausgewählt sind, und die entsprechenden IDs hinzufügen
                            if (CheckBoxDoppelzimmer.IsChecked == true)
                                eigenschaftenIds.Add(7); // Doppelzimmer
                            if (CheckBoxSuite.IsChecked == true)
                                eigenschaftenIds.Add(8); // Suite
                            if (CheckBoxHauptstrasse.IsChecked == true)
                                eigenschaftenIds.Add(9); // Sicht zur Hauptstraße heraus
                            if (CheckBoxKuehlschrank.IsChecked == true)
                                eigenschaftenIds.Add(10); // Kühlschrank
                            if (CheckBoxTerasse.IsChecked == true)
                                eigenschaftenIds.Add(11); // Terasse
                            if (CheckBoxGrosserBalkon.IsChecked == true)
                                eigenschaftenIds.Add(12); // Großer Balkon
                            if (CheckBoxKleinerBalkon.IsChecked == true)
                                eigenschaftenIds.Add(13); // Kleiner Balkon

                            // Zimmer in die Tabelle `zimmer` einfügen
                            string insertZimmerQuery = "INSERT INTO zimmer (id, gebaeude_id) VALUES (@zimmerId, @gebaeudeId)";
                            using (MySqlCommand command = new MySqlCommand(insertZimmerQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@zimmerId", zimmerId);
                                command.Parameters.AddWithValue("@gebaeudeId", gebaeudeId);
                                command.ExecuteNonQuery();
                            }

                            // Verknüpfung zwischen Zimmer und Eigenschaften in `zimmer_hat_eigenschaften` einfügen
                            foreach (int eigenschaftId in eigenschaftenIds)
                            {
                                string insertZimmerEigenschaftQuery = @"
                            INSERT INTO zimmer_hat_eigenschaften (zimmer_id, eigenschaften_id)
                            VALUES (@zimmerId, @eigenschaftId)";
                                using (MySqlCommand command = new MySqlCommand(insertZimmerEigenschaftQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@zimmerId", zimmerId);
                                    command.Parameters.AddWithValue("@eigenschaftId", eigenschaftId);
                                    command.ExecuteNonQuery();
                                }
                            }

                            // Transaktion erfolgreich abschließen
                            transaction.Commit();

                            MessageBox.Show("Zimmer erfolgreich eingetragen!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            // Transaktion zurückrollen, falls ein Fehler auftritt
                            transaction.Rollback();
                            MessageBox.Show("Fehler beim Eintragen des Zimmers: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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