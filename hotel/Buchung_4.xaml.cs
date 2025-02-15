using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MySqlConnector;

namespace hotel
{ 
    public partial class Buchung_3 : Page
    {
        
        private int kundenID;
        private DateTime startDatum;
        private DateTime endDatum;


        private string connectionString = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";

        
        private List<int> ausgewaehlteZimmer = new List<int>();

        
        public Buchung_3(int kundenID, DateTime startDatum, DateTime endDatum)
        {

            InitializeComponent();
            this.kundenID = kundenID;
            this.startDatum = startDatum;
            this.endDatum = endDatum;

            
            kundenIDTextBlock.Text = $"Kunden-ID: {kundenID}";
            startDatumTextBlock.Text = $"Startdatum: {startDatum.ToString("dd.MM.yyyy")}"; // Europäisches Format
            endDatumTextBlock.Text = $"Enddatum: {endDatum.ToString("dd.MM.yyyy")}";     // Europäisches Format

           
            LadeVerfuegbareZimmer();
            LadeLeistungen();
        }

        public class LeistungViewModel : INotifyPropertyChanged
        {
            private DateTime _startDatum;
            private DateTime _endDatum;
            private bool _isSelected;

            public int LeistungID { get; set; }
            public string LeistungName { get; set; }
            public decimal Preis { get; set; }

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
                        if (value < BuchungStartDatum)
                        {
                            
                            MessageBox.Show($"Das Startdatum der Zusatzleistung '{LeistungName}' darf nicht vor dem {BuchungStartDatum.ToString("dd.MM.yyyy")} liegen!",
                                "Ungültiges Startdatum", MessageBoxButton.OK, MessageBoxImage.Warning);

                            value = BuchungStartDatum; 
                        }

                        _startDatum = value;
                        OnPropertyChanged(nameof(StartDatum));
                        OnPropertyChanged(nameof(StartDatumLabel)); 
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
                        OnPropertyChanged(nameof(EndDatumLabel)); 
                    }
                }
            }

            public DateTime BuchungStartDatum { get; set; }
            public DateTime BuchungEndDatum { get; set; }

            
            public string StartDatumLabel => StartDatum.ToString("dd.MM.yyyy");
            public string EndDatumLabel => EndDatum.ToString("dd.MM.yyyy");

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        
        private void LadeVerfuegbareZimmer()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                SELECT 
                    zimmer.id AS 'Zimmer Nummer', 
                    gebaeude.id AS 'Gebäude Nummer', 
                    adresse.strasse, 
                    adresse.plz,
                    GROUP_CONCAT(eigenschaften.eigenschaft SEPARATOR ', ') AS 'Eigenschaften'
                FROM zimmer
                INNER JOIN gebaeude ON zimmer.gebaeude_id = gebaeude.id
                INNER JOIN adresse ON gebaeude.adress_id = adresse.id
                LEFT JOIN zimmer_hat_eigenschaften ON zimmer.id = zimmer_hat_eigenschaften.zimmer_id
                LEFT JOIN eigenschaften ON zimmer_hat_eigenschaften.eigenschaften_id = eigenschaften.id
                WHERE zimmer.id NOT IN 
                (
                    SELECT DISTINCT buchung.zimmer_id
                    FROM buchung
                    WHERE buchung.datum BETWEEN @startDatum AND @endDatum
                )
                GROUP BY zimmer.id;";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@startDatum", startDatum);
                    cmd.Parameters.AddWithValue("@endDatum", endDatum);

                    DataTable dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());
                    zimmerDataGrid.ItemsSource = dataTable.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Laden der Zimmer: " + ex.Message);
                }
                finally
                {
                   
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }

       
        private void LadeLeistungen()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id, leistung, preis FROM leistungen;";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    DataTable dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    
                    List<LeistungViewModel> leistungen = new List<LeistungViewModel>();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        DateTime leistungStart = startDatum; 
                        DateTime leistungEnd = endDatum;     

                        
                        if (leistungStart < startDatum)
                        {
                            
                            string startDatumFormatiert = startDatum.ToString("dd.MM.yyyy");
                            MessageBox.Show($"Das Startdatum der Zusatzleistung '{row["leistung"]}' darf nicht vor dem {startDatumFormatiert} liegen!",
                                "Ungültiges Startdatum", MessageBoxButton.OK, MessageBoxImage.Warning);

                            leistungStart = startDatum; 
                        }

                        leistungen.Add(new LeistungViewModel
                        {
                            LeistungID = Convert.ToInt32(row["id"]),
                            LeistungName = row["leistung"].ToString(),
                            Preis = Convert.ToDecimal(row["preis"]),
                            IsSelected = false, 
                            StartDatum = leistungStart,
                            EndDatum = leistungEnd,
                            BuchungStartDatum = startDatum,
                            BuchungEndDatum = endDatum
                        });
                    }

                    
                    leistungenListView.ItemsSource = leistungen;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Laden der Leistungen: " + ex.Message);
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }


        private void ZimmerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            ausgewaehlteZimmer.Clear();

           
            try
            {
                foreach (DataRowView selectedRow in zimmerDataGrid.SelectedItems)
                {
                    int zimmernummer = Convert.ToInt32(selectedRow["Zimmer Nummer"]);
                    ausgewaehlteZimmer.Add(zimmernummer);
                }
            }
            catch
            {
                MessageBox.Show("Es konnte kein Zimmer ausgewählt werden.");
            }
        }

        
        private void ZimmerAuswaehlen_Click(object sender, RoutedEventArgs e)
        {
            if (ausgewaehlteZimmer.Count > 0)
            {
                
                foreach (LeistungViewModel leistung in leistungenListView.ItemsSource)
                {
                    if (leistung.IsSelected && leistung.EndDatum > endDatum)
                    {
                        MessageBox.Show($"Das Enddatum der Zusatzleistung '{leistung.LeistungName}' darf nicht nach dem {endDatum.ToShortDateString()} liegen!",
                            "Ungültiges Enddatum", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return; 
                    }
                }

                
                int rechnungsID = ErstelleRechnung(startDatum, endDatum);

                if (rechnungsID > 0)
                {
                    
                    List<int> buchungsIDs = new List<int>();

                    
                    foreach (int zimmernummer in ausgewaehlteZimmer)
                    {
                       
                        for (DateTime datum = startDatum; datum <= endDatum; datum = datum.AddDays(1))
                        {
                            int buchungID = erstelleBuchung(zimmernummer, rechnungsID, datum);

                            if (buchungID > 0)
                            {
                                buchungsIDs.Add(buchungID);
                            }
                        }
                    }

                    
                    SpeichereLeistungen(rechnungsID);

                    MessageBox.Show($"{buchungsIDs.Count} Buchungen erfolgreich erstellt!",
                        "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Fehler beim Erstellen der Rechnung.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie mindestens ein Zimmer aus.",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        
        private void SpeichereLeistungen(int rechnungsID)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    foreach (LeistungViewModel leistung in leistungenListView.ItemsSource)
                    {
                        if (leistung.IsSelected)
                        {
                            
                            string getBuchungenQuery = @"
                        SELECT id, datum 
                        FROM buchung 
                        WHERE rechnung_id = @rechnungsID 
                        AND datum BETWEEN @startDatum AND @endDatum;";

                            MySqlCommand getBuchungenCmd = new MySqlCommand(getBuchungenQuery, connection);
                            getBuchungenCmd.Parameters.AddWithValue("@rechnungsID", rechnungsID);
                            getBuchungenCmd.Parameters.AddWithValue("@startDatum", leistung.StartDatum);
                            getBuchungenCmd.Parameters.AddWithValue("@endDatum", leistung.EndDatum);

                            List<int> buchungsIDs = new List<int>();

                            
                            using (MySqlDataReader reader = getBuchungenCmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int buchungID = reader.GetInt32("id");
                                    buchungsIDs.Add(buchungID);
                                }
                            }

                          
                            foreach (int buchungID in buchungsIDs)
                            {
                                string insertLeistungQuery = @"
                            INSERT INTO buchung_hat_leistung (buchung_id, leistung_id, anzahl)
                            VALUES (@buchungID, @leistungID, @anzahl);";

                                MySqlCommand insertLeistungCmd = new MySqlCommand(insertLeistungQuery, connection);
                                insertLeistungCmd.Parameters.AddWithValue("@buchungID", buchungID);
                                insertLeistungCmd.Parameters.AddWithValue("@leistungID", leistung.LeistungID);
                                insertLeistungCmd.Parameters.AddWithValue("@anzahl", 1); 

                                insertLeistungCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Speichern der Leistungen: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        
        private int ErstelleRechnung(DateTime startDatum, DateTime endDatum)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string insertRechnungQuery = @"
                        INSERT INTO rechnung (anfang, ende, kunden_id)
                        VALUES (@startDatum, @endDatum, @kundenID);
                        SELECT LAST_INSERT_ID();";

                    MySqlCommand rechnungCmd = new MySqlCommand(insertRechnungQuery, connection);
                    rechnungCmd.Parameters.AddWithValue("@startDatum", startDatum);
                    rechnungCmd.Parameters.AddWithValue("@endDatum", endDatum);
                    rechnungCmd.Parameters.AddWithValue("@kundenID", kundenID);

                    int rechnungsID = Convert.ToInt32(rechnungCmd.ExecuteScalar());
                    return rechnungsID;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Erstellen der Rechnung: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return -1;
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }

      
        private int erstelleBuchung(int zimmernummer, int rechnungsID, DateTime datum)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                   
                    string checkBuchungQuery = @"
                SELECT COUNT(*) 
                FROM buchung 
                WHERE datum = @datum AND zimmer_id = @zimmernummer;";

                    MySqlCommand checkCmd = new MySqlCommand(checkBuchungQuery, connection);
                    checkCmd.Parameters.AddWithValue("@datum", datum);
                    checkCmd.Parameters.AddWithValue("@zimmernummer", zimmernummer);

                    int anzahlBuchungen = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (anzahlBuchungen > 0)
                    {
                       
                        MessageBox.Show($"Das Zimmer {zimmernummer} ist am {datum.ToShortDateString()} bereits gebucht.",
                            "Warnung", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return -1;
                    }

                    
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            
                            string insertBuchungQuery = @"
                        INSERT INTO buchung (datum, rechnung_id, zimmer_id)
                        VALUES (@datum, @rechnungsID, @zimmernummer);
                        SELECT LAST_INSERT_ID();";

                            MySqlCommand buchungCmd = new MySqlCommand(insertBuchungQuery, connection, transaction);
                            buchungCmd.Parameters.AddWithValue("@datum", datum);
                            buchungCmd.Parameters.AddWithValue("@rechnungsID", rechnungsID);
                            buchungCmd.Parameters.AddWithValue("@zimmernummer", zimmernummer);

                            
                            int buchungID = Convert.ToInt32(buchungCmd.ExecuteScalar());

                            
                            transaction.Commit();

                            
                            MessageBox.Show($"Buchung für Zimmer {zimmernummer} am {datum.ToShortDateString()} erfolgreich erstellt. Buchungs-ID: {buchungID}",
                                "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                            return buchungID;
                        }
                        catch (Exception ex)
                        {
                            
                            transaction.Rollback();
                            MessageBox.Show($"Fehler beim Buchen des Zimmers {zimmernummer} am {datum.ToShortDateString()}: {ex.Message}",
                                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                            return -1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler bei der Verbindung zur Datenbank: " + ex.Message,
                        "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return -1;
                }
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DatePicker datePicker)
            {
               
                var listViewItem = FindAncestor<ListViewItem>(datePicker);
                if (listViewItem == null)
                    return;

               
                Label label = null;
                if (datePicker.Name == "fernsehen_anfang")
                {
                    label = FindChild<Label>(listViewItem, "label_fernsehen_anfang");
                }
                else if (datePicker.Name == "fernsehen_ende")
                {
                    label = FindChild<Label>(listViewItem, "label_fernsehen_ende");
                }

                
                if (label != null)
                {
                    label.Content = datePicker.SelectedDate.HasValue
                        ? datePicker.SelectedDate.Value.ToString("dd.MM.yyyy") 
                        : "Datum eintragen";
                }
            }
        }

        
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null && !(current is T))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }

        
        private static T FindChild<T>(DependencyObject parent, string childName) where T : FrameworkElement
        {
            if (parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T element && element.Name == childName)
                {
                    return element;
                }

                T foundChild = FindChild<T>(child, childName);
                if (foundChild != null)
                    return foundChild;
            }
            return null;
        }


    }
}