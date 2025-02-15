using System;
using System.Collections.Generic;
using MySqlConnector; 
using System.Windows;
using System.Windows.Controls;

namespace hotel
{
    public partial class Zimmer_eintragen : Page
    {
       
        private string connectionString = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";

        public Zimmer_eintragen()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            
                            int zimmerId = int.Parse(TextboxzimmerID.Text);
                            int gebaeudeId = int.Parse(TextboxgebaeudeID.Text);

                           
                            string checkZimmerQuery = "SELECT COUNT(*) FROM zimmer WHERE id = @zimmerId";
                            using (MySqlCommand checkCommand = new MySqlCommand(checkZimmerQuery, connection, transaction))
                            {
                                checkCommand.Parameters.AddWithValue("@zimmerId", zimmerId);
                                int zimmerCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                                if (zimmerCount > 0)
                                {
                                    MessageBox.Show("Das Zimmer existiert bereits in der Datenbank.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }

                            
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

                            
                            List<int> eigenschaftenIds = new List<int>();

                            
                            if (CheckBoxDoppelzimmer.IsChecked == true)
                                eigenschaftenIds.Add(7); 
                            if (CheckBoxSuite.IsChecked == true)
                                eigenschaftenIds.Add(8); 
                            if (CheckBoxHauptstrasse.IsChecked == true)
                                eigenschaftenIds.Add(9); 
                            if (CheckBoxKuehlschrank.IsChecked == true)
                                eigenschaftenIds.Add(10);
                            if (CheckBoxTerasse.IsChecked == true)
                                eigenschaftenIds.Add(11); 
                            if (CheckBoxGrosserBalkon.IsChecked == true)
                                eigenschaftenIds.Add(12); 
                            if (CheckBoxKleinerBalkon.IsChecked == true)
                                eigenschaftenIds.Add(13); 

                            
                            string insertZimmerQuery = "INSERT INTO zimmer (id, gebaeude_id) VALUES (@zimmerId, @gebaeudeId)";
                            using (MySqlCommand command = new MySqlCommand(insertZimmerQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@zimmerId", zimmerId);
                                command.Parameters.AddWithValue("@gebaeudeId", gebaeudeId);
                                command.ExecuteNonQuery();
                            }

                            
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

                            
                            transaction.Commit();

                            MessageBox.Show("Zimmer erfolgreich eingetragen!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                           
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