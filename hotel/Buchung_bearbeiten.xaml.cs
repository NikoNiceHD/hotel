using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MySqlConnector;

namespace hotel
{
    public partial class Buchung_bearbeiten : Page
    {
        private string connectionString = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";
        private DataView buchungenDataView; 

        private int ausgewaehlteBuchungID = -1;

        public Buchung_bearbeiten()
        {
            InitializeComponent();
            LadeBuchungen();
        }

        private void LadeBuchungen()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
    SELECT buchung.id AS 'Buchungs-ID', 
           DATE_FORMAT(buchung.datum, '%d.%m.%Y') AS 'Datum', 
           buchung.rechnung_id AS 'Rechnungs-ID', 
           buchung.zimmer_id AS 'Zimmer-ID',
           COALESCE(kunde.vorname, 'Unbekannt') AS 'Kunden-Vorname', 
           COALESCE(kunde.nachname, 'Unbekannt') AS 'Kunden-Nachname'
    FROM buchung
    LEFT JOIN rechnung ON buchung.rechnung_id = rechnung.id
    LEFT JOIN kunde ON rechnung.kunden_id = kunde.id
    ORDER BY buchung.id;";
                    

                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    DataTable dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    
                    foreach (DataRow row in dataTable.Rows)
                    {
                        if (row["Datum"] != DBNull.Value)
                        {
                            DateTime datum = Convert.ToDateTime(row["Datum"]);
                            row["Datum"] = datum.ToString("dd.MM.yyyy");
                        }
                    }

                    buchungenDataView = dataTable.DefaultView;
                    buchungenDataGrid.ItemsSource = buchungenDataView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Laden der Buchungen: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SucheTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (buchungenDataView != null)
            {
               
                string filterText = sucheTextBox.Text.Trim();
                if (string.IsNullOrEmpty(filterText))
                {
                    buchungenDataView.RowFilter = "";
                }
                else
                {
                   
                    buchungenDataView.RowFilter = $@"
                CONVERT([Buchungs-ID], 'System.String') LIKE '%{filterText}%' OR
                [Kunden-Vorname] LIKE '%{filterText}%' OR
                [Kunden-Nachname] LIKE '%{filterText}%' OR
                ([Kunden-Vorname] + ' ' + [Kunden-Nachname]) LIKE '%{filterText}%'";
                }
            }
        }


        private void BuchungenDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            
            if (buchungenDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = buchungenDataGrid.SelectedItem as DataRowView;
                if (selectedRow != null)
                {
                    ausgewaehlteBuchungID = Convert.ToInt32(selectedRow["Buchungs-ID"]);
                }
            }
        }

        private void BearbeitenButton_Click(object sender, RoutedEventArgs e)
        {
            if (ausgewaehlteBuchungID > 0)
            {
                Buchung_bearbeiten_2 bearbeitenDetailsPage = new Buchung_bearbeiten_2(ausgewaehlteBuchungID);
                this.NavigationService.Navigate(bearbeitenDetailsPage);
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie eine Buchung aus.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
