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
        private DataTable dataTable; 

        public Buchung()
        {
            InitializeComponent();
            LoadData("SELECT * FROM azanik.kunde"); 
        }

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
                    dtgrid.DataContext = dataTable; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Daten: " + ex.Message);
            }
        }


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
                    LoadData("SELECT * FROM azanik.kunde"); 
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadData("SELECT * FROM azanik.kunde");
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dtgrid.SelectedItem != null)
            {
                DataRowView selectedRow = dtgrid.SelectedItem as DataRowView;

                if (selectedRow != null)
                {
                    int kundenID = Convert.ToInt32(selectedRow["id"]); 

                    Buchung_erstellen buchung_erstellen = new Buchung_erstellen(kundenID);
                    this.NavigationService.Navigate(buchung_erstellen);
                }
            }
        }
    }
}