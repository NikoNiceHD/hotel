using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySqlConnector;

namespace hotel
{
    public partial class Buchung : Page
    {
        string connectstring = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";
        private DataTable dataTable; // Datenquelle für das DataGrid

        public Buchung()
        {
            InitializeComponent();
            LoadData("SELECT * FROM azanik.kunde"); // Lade alle Kunden beim Start
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

        // Suchfunktion bei Drücken der Enter-Taste
        private void SearchTextBox(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string search = searchTextBox.Text.Trim();

                if (!string.IsNullOrEmpty(search))
                {
                    string query = $"SELECT * FROM azanik.kunde WHERE vorname LIKE '%{search}%' OR nachname LIKE '%{search}%'";
                    LoadData(query);
                }
                else
                {
                    MessageBox.Show("Bitte geben Sie einen Suchbegriff ein.");
                    LoadData("SELECT * FROM azanik.kunde"); // Lade alle Daten, wenn die Suche leer ist
                }
            }
        }

        // Methode zum Laden der Daten (wird vom Button "Daten laden" aufgerufen)
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadData("SELECT * FROM azanik.kunde");
        }

        // Methode zum Speichern der Änderungen in die Datenbank
       
            
    }
}