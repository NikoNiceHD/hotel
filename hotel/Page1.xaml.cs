using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySqlConnector;

namespace hotel
{
    public partial class Page1 : Page
    {
        string connectstring = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";

        public Page1()
        {
            InitializeComponent();
            LoadData("SELECT * FROM azanik.kunde");
        }

       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
        }

        
        private void SearchTextBox(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string search = searchTextBox.Text.Trim();

                if (!string.IsNullOrEmpty(search))
                {
                   
                    string query = $"SELECT * FROM azanik.kunde WHERE vorname LIKE '%{search}%'";
                    LoadData(query);
                }
                else
                {
                    MessageBox.Show("Bitte geben Sie einen Suchbegriff ein.");
                }
            }
        }

        
        private void LoadData(string query)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectstring))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    dtgrid.DataContext = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Daten: " + ex.Message);
            }
        }
    }
}