using System;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace hotel
{
    public partial class Kunden_eintragen : Page
    {
        string connectstring = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";

        public Kunden_eintragen()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Überprüfen, ob alle Felder ausgefüllt sind
            if (string.IsNullOrEmpty(textbox_vorname.Text) || string.IsNullOrEmpty(textbox_nachname.Text) ||
                datepicker_geburtstag.SelectedDate == null || string.IsNullOrEmpty(textbox_plz.Text) ||
                string.IsNullOrEmpty(textbox_ort.Text) || string.IsNullOrEmpty(textbox_strasse_hausnummer.Text) ||
                datepicker_einzugsdatum.SelectedDate == null)
            {
                MessageBox.Show("Keine leeren Felder möglich.");
                return;
            }

            // Überprüfen, ob die Postleitzahl eine fünfstellige Zahl ist
            if (!int.TryParse(textbox_plz.Text, out int plz) || textbox_plz.Text.Length != 5)
            {
                MessageBox.Show("Die Postleitzahl muss eine fünfstellige Zahl sein.");
                return;
            }

            // Überprüfen, ob die Straße und Hausnummer korrekt eingegeben wurden
            string[] strasseUndHausnummer = textbox_strasse_hausnummer.Text.Trim().Split(' ');

            // Mindestens zwei Teile (Straße und Hausnummer) müssen vorhanden sein
            if (strasseUndHausnummer.Length < 2)
            {
                MessageBox.Show("Bitte geben Sie sowohl die Straße als auch die Hausnummer ein (z.B. 'Musterstraße 12').");
                return;
            }

            // Überprüfen, ob der letzte Teil eine Zahl ist (Hausnummer)
            string hausnummerTeil = strasseUndHausnummer[strasseUndHausnummer.Length - 1];
            if (!int.TryParse(hausnummerTeil, out int hausnummer))
            {
                MessageBox.Show("Die Hausnummer muss eine Zahl sein (z.B. 'Musterstraße 12').");
                return;
            }

            /*
             DateTime altgenug = DateTime.Now.AddYears(-18);
             if (datepicker_geburtstag.SelectedDate > altgenug)
             {
                 MessageBox.Show("Sie müssen mindestens 18 Jahre alt sein, um ein Zimmer buchen zu können.");
                 return;
             }

             // Überprüfe, ob das Einzugsdatum in der Zukunft liegt
             if (datepicker_einzugsdatum.SelectedDate < DateTime.Now)
             {
                 MessageBox.Show("Es können keine Zimmer in der Vergangenheit gebucht werden.");
                 return;
             }
            */
            using (MySqlConnection conn = new MySqlConnection(connectstring))
            {
                conn.Open();

                
                using (MySqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        
                        string insertKundeQuery = @"
                            INSERT INTO kunde (vorname, nachname, geburtsdatum)
                            VALUES (@vorname, @nachname, @geburtsdatum);
                            SELECT LAST_INSERT_ID();";

                        int kundenId;

                        using (MySqlCommand cmd = new MySqlCommand(insertKundeQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@vorname", textbox_vorname.Text);
                            cmd.Parameters.AddWithValue("@nachname", textbox_nachname.Text);
                            cmd.Parameters.AddWithValue("@geburtsdatum", datepicker_geburtstag.SelectedDate.Value.ToString("yyyy-MM-dd"));

                            
                            kundenId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        
                        string insertPlzQuery = @"
                            INSERT IGNORE INTO plz (plz, ort)
                            VALUES (@plz, @ort);";

                        using (MySqlCommand cmd = new MySqlCommand(insertPlzQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@plz", textbox_plz.Text);
                            cmd.Parameters.AddWithValue("@ort", textbox_ort.Text);

                           
                            cmd.ExecuteNonQuery();
                        }

                        
                        string insertAdresseQuery = @"
                            INSERT INTO adresse (strasse, plz)
                            VALUES (@strasse, @plz);
                            SELECT LAST_INSERT_ID();";

                        int adressId;

                        using (MySqlCommand cmd = new MySqlCommand(insertAdresseQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@strasse", textbox_strasse_hausnummer.Text);  
                            cmd.Parameters.AddWithValue("@plz", textbox_plz.Text);

                            
                            adressId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        
                        string insertKundeHatAdresseQuery = @"
                            INSERT INTO kunde_hat_adresse (adress_id, kunden_id, datum)
                            VALUES (@adress_id, @kunden_id, @datum)";

                        using (MySqlCommand cmd = new MySqlCommand(insertKundeHatAdresseQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@adress_id", adressId);
                            cmd.Parameters.AddWithValue("@kunden_id", kundenId);
                            cmd.Parameters.AddWithValue("@datum", datepicker_einzugsdatum.SelectedDate.Value.ToString("yyyy-MM-dd"));

                            
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                
                                transaction.Commit();
                                MessageBox.Show("Kunde, PLZ, Adresse und Verknüpfung erfolgreich eingetragen!");
                            }
                            else
                            {
                                
                                transaction.Rollback();
                                MessageBox.Show("Fehler beim Eintragen der Verknüpfung.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        
                        transaction.Rollback();
                        MessageBox.Show("Fehler: " + ex.Message);
                    }
                }
            }
        }


        
    }
}