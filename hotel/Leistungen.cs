using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySqlConnector;
using System.Windows;
using System.ComponentModel;

namespace hotel
{
    internal class Leistungen
    {
        private string connectionString = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";

        public class LeistungViewModel : INotifyPropertyChanged
        {
            private DateTime _startDatum;
            private DateTime _endDatum;
            private bool _isSelected;

            public int LeistungID { get; set; } // ID der Leistung
            public string LeistungName { get; set; } // Name der Leistung
            public decimal Preis { get; set; } // Preis der Leistung

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected != value)
                    {
                        _isSelected = value;
                        OnPropertyChanged(nameof(IsSelected));
                    }
                }
            }

            public DateTime StartDatum
            {
                get => _startDatum;
                set
                {
                    if (_startDatum != value)
                    {
                        _startDatum = value;
                        OnPropertyChanged(nameof(StartDatum));
                        OnPropertyChanged(nameof(StartDatumLabel)); // Aktualisiere das Label
                    }
                }
            }

            public DateTime EndDatum
            {
                get => _endDatum;
                set
                {
                    if (_endDatum != value)
                    {
                        _endDatum = value;
                        OnPropertyChanged(nameof(EndDatum));
                        OnPropertyChanged(nameof(EndDatumLabel)); // Aktualisiere das Label
                    }
                }
            }

            // Neue Eigenschaften für die Labels
            public string StartDatumLabel => StartDatum.ToString("dd.MM.yyyy");
            public string EndDatumLabel => EndDatum.ToString("dd.MM.yyyy");

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Methode zum Laden der Leistungen
        public List<LeistungViewModel> LadeLeistungen(DateTime startDatum, DateTime endDatum)
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
                            IsSelected = false, // Standardmäßig nicht ausgewählt
                            StartDatum = startDatum, // Standardwert: Startdatum der Buchung
                            EndDatum = endDatum     // Standardwert: Enddatum der Buchung
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