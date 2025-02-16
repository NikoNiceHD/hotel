using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace hotel
{
    public partial class Rechnung_2 : Page
    {
        private string connectionString = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";
        private int rechnungsId;
        private CultureInfo culture = new CultureInfo("de-DE"); 

        public Rechnung_2(int rechnungsId)
        {
            InitializeComponent();
            this.rechnungsId = rechnungsId;
            LadeRechnungsDetails();
        }

        private void LadeRechnungsDetails()
        {
            decimal totalGesamt = 0;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string baseQuery = @"
                SELECT 
                    k.vorname, 
                    k.nachname,
                    r.id,
                    r.anfang,
                    r.ende,
                    CAST(DATEDIFF(r.ende, r.anfang) + 1 AS SIGNED) AS Gesamtdauer
                FROM rechnung r
                JOIN kunde k ON r.kunden_id = k.id
                WHERE r.id = @RechnungsID";

                    MySqlCommand baseCmd = new MySqlCommand(baseQuery, connection);
                    baseCmd.Parameters.AddWithValue("@RechnungsID", rechnungsId);
                    DataTable baseData = new DataTable();
                    baseData.Load(baseCmd.ExecuteReader());

                    string featuresQuery = @"
                SELECT DISTINCT
                    e.eigenschaft AS Vorteil,
                    CAST(DATEDIFF(r.ende, r.anfang) + 1 AS SIGNED) AS Tage,
                    e.preis AS Tagespreis,
                    CAST((DATEDIFF(r.ende, r.anfang) + 1) * e.preis AS DECIMAL(10,2)) AS Gesamt
                FROM rechnung r
                JOIN buchung b ON r.id = b.rechnung_id
                JOIN zimmer z ON b.zimmer_id = z.id
                JOIN zimmer_hat_eigenschaften ze ON z.id = ze.zimmer_id
                JOIN eigenschaften e ON ze.eigenschaften_id = e.id
                WHERE r.id = @RechnungsID";

                    MySqlCommand featuresCmd = new MySqlCommand(featuresQuery, connection);
                    featuresCmd.Parameters.AddWithValue("@RechnungsID", rechnungsId);
                    DataTable featuresData = new DataTable();
                    featuresData.Load(featuresCmd.ExecuteReader());

                    string servicesQuery = @"
                SELECT 
                    l.leistung AS Leistung,
                    CAST(SUM(bl.anzahl) AS SIGNED) AS Anzahl,
                    l.preis AS Einzelpreis,
                    CAST(SUM(bl.anzahl * l.preis) AS DECIMAL(10,2)) AS Gesamt
                FROM buchung_hat_leistung bl
                JOIN leistungen l ON bl.leistung_id = l.id
                JOIN buchung b ON bl.buchung_id = b.id
                JOIN rechnung r ON b.rechnung_id = r.id
                WHERE b.rechnung_id = @RechnungsID
                GROUP BY l.leistung, l.preis";

                    MySqlCommand servicesCmd = new MySqlCommand(servicesQuery, connection);
                    servicesCmd.Parameters.AddWithValue("@RechnungsID", rechnungsId);
                    DataTable servicesData = new DataTable();
                    servicesData.Load(servicesCmd.ExecuteReader());

                    DataTable combinedData = new DataTable();
                    combinedData.Columns.Add("Typ", typeof(string));
                    combinedData.Columns.Add("Beschreibung", typeof(string));
                    combinedData.Columns.Add("Tage/Anzahl", typeof(int));
                    combinedData.Columns.Add("Einzelpreis", typeof(string)); 
                    combinedData.Columns.Add("Gesamt", typeof(string)); 

                    
                    foreach (DataRow row in featuresData.Rows)
                    {
                        decimal einzelpreis = Convert.ToDecimal(row["Tagespreis"]);
                        decimal gesamt = Convert.ToDecimal(row["Gesamt"]);

                        combinedData.Rows.Add(
                            "Vorteil",
                            row["Vorteil"],
                            Convert.ToInt32(row["Tage"]),
                            einzelpreis.ToString("C", culture), 
                            gesamt.ToString("C", culture) 
                        );
                        totalGesamt += gesamt;
                    }

                    
                    foreach (DataRow row in servicesData.Rows)
                    {
                        decimal einzelpreis = Convert.ToDecimal(row["Einzelpreis"]);
                        decimal gesamt = Convert.ToDecimal(row["Gesamt"]);

                        combinedData.Rows.Add(
                            "Leistung",
                            row["Leistung"],
                            Convert.ToInt32(row["Anzahl"]),
                            einzelpreis.ToString("C", culture),
                            gesamt.ToString("C", culture) 
                        );
                        totalGesamt += gesamt;
                    }

                   
                    if (baseData.Rows.Count > 0)
                    {
                        DataRow baseRow = baseData.Rows[0];
                        rechnungsDetailsTextBlock.Text = $"Kunde: {baseRow["vorname"]} {baseRow["nachname"]}\n"
                                                       + $"Rechnungs-ID: {baseRow["id"]}\n"
                                                       + $"Buchungsdauer: {Convert.ToInt32(baseRow["Gesamtdauer"])} Tage\n"
                                                       + $"Zeitraum: {((DateTime)baseRow["anfang"]).ToShortDateString()} - "
                                                       + $"{((DateTime)baseRow["ende"]).ToShortDateString()}";

                        gesamtpreisTextBlock.Text = $"Gesamtpreis: {totalGesamt.ToString("C", culture)}";
                        combinedDataGrid.ItemsSource = combinedData.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Laden der Daten: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
