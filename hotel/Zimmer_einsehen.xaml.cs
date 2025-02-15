using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace hotel
{
    public partial class Zimmer_einsehen : Page
    {
        string connectstring = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";

        public Zimmer_einsehen()
        {
            InitializeComponent();
            LoadZimmerData(); // Lade die Daten beim Initialisieren der Seite
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Lade die Daten, wenn das Datum geändert wird
            LoadZimmerData();
        }

        private void LoadZimmerData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectstring))
                {
                    conn.Open();

                    // Hole das ausgewählte Datum aus dem DatePicker
                    DateTime selectedDate = datePicker.SelectedDate ?? DateTime.Today;

                    // Query für freie Zimmer
                    string queryFreieZimmer = @"
                        SELECT z.id, z.gebaeude_id 
                        FROM zimmer z
                        LEFT JOIN buchung b ON z.id = b.zimmer_id AND b.datum = @selectedDate
                        WHERE b.id IS NULL";

                    // Query für belegte Zimmer
                    string queryBelegteZimmer = @"
                        SELECT DISTINCT z.id, z.gebaeude_id 
                        FROM zimmer z
                        INNER JOIN buchung b ON z.id = b.zimmer_id
                        WHERE b.datum = @selectedDate";

                    // Lade freie Zimmer
                    MySqlCommand cmdFreie = new MySqlCommand(queryFreieZimmer, conn);
                    cmdFreie.Parameters.AddWithValue("@selectedDate", selectedDate.ToString("yyyy-MM-dd"));
                    DataTable dtFreie = new DataTable();
                    dtFreie.Load(cmdFreie.ExecuteReader());
                    dtgridFreieZimmer.ItemsSource = dtFreie.DefaultView;

                    // Lade belegte Zimmer
                    MySqlCommand cmdBelegte = new MySqlCommand(queryBelegteZimmer, conn);
                    cmdBelegte.Parameters.AddWithValue("@selectedDate", selectedDate.ToString("yyyy-MM-dd"));
                    DataTable dtBelegte = new DataTable();
                    dtBelegte.Load(cmdBelegte.ExecuteReader());
                    dtgridBelegteZimmer.ItemsSource = dtBelegte.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Daten: " + ex.Message);
            }
        }
    }
}