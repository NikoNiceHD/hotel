using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace hotel
{
    /// <summary>
    /// Interaktionslogik für Zimmer_einsehen.xaml
    /// </summary>
    public partial class Zimmer_einsehen : Page
    {
        string connectstring = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";
        private DataTable dataTable;

        public Zimmer_einsehen()
        {
            InitializeComponent();
            
            LoadData(@"SELECT zimmer.id, zimmer.gebaeude_id
                       FROM zimmer
                       INNER JOIN gebaeude ON zimmer.gebaeude_id = gebaeude.id;");
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
    }
}
