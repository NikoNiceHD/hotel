using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySqlConnector;
using System.Windows;

namespace hotel
{
    internal class Leistungen
    {
        private string connectionString = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";

        public class LeistungViewModel
        {
            public int LeistungID { get; set; } // ID der Leistung
            public string LeistungName { get; set; } // Name der Leistung
            public decimal Preis { get; set; } // Preis der Leistung
            public bool IsSelected { get; set; } // Auswahlstatus (Checkbox)
        }

        // Methode zum Laden der Leistungen
        public List<LeistungViewModel> LadeLeistungen()
        {
            List<LeistungViewModel> leistungen = new List<LeistungViewModel>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT id, leistung, preis FROM leistungen;";
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    DataTable dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    // Konvertiere die Daten in eine Liste von LeistungViewModel
                    leistungen = dataTable.AsEnumerable()
                        .Select(row => new LeistungViewModel
                        {
                            LeistungID = row.Field<int>("id"),
                            LeistungName = row.Field<string>("leistung"),
                            Preis = row.Field<decimal>("preis"),
                            IsSelected = false // Standardmäßig nicht ausgewählt
                        }).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Laden der Leistungen: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return leistungen;
        }

        // Methode zum Erstellen einer Zeichenkette mit den ausgewählten Leistungen
        public string GetAusgewaehlteLeistungen(List<LeistungViewModel> leistungen)
        {
            // Filtere die ausgewählten Leistungen
            var ausgewaehlteLeistungen = leistungen.Where(l => l.IsSelected).ToList();

            if (ausgewaehlteLeistungen.Count == 0)
            {
                return "Keine Leistungen ausgewählt.";
            }

            // Erstelle eine Zeichenkette mit den Namen der ausgewählten Leistungen
            string leistungenText = "Ausgewählte Leistungen:\n";
            foreach (var leistung in ausgewaehlteLeistungen)
            {
                leistungenText += $"- {leistung.LeistungName} ({leistung.Preis:C})\n";
            }

            return leistungenText;
        }
    }
}