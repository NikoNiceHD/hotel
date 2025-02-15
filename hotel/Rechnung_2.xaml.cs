using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace hotel
{
    public partial class Rechnung_2 : Page
    {
        private string connectionString = "server=drip-tuxedo.eu;uid=azanik;pwd=Fortnite6969!;database=azanik";
        private int rechnungsId;

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

                    // Basis-Rechnungsdaten
                    string baseQuery = @"
                SELECT 
                    k.vorname, 
                    k.nachname,
                    r.id,
                    r.anfang,
                    r.ende,
                    DATEDIFF(r.ende, r.anfang) + 1 AS Gesamtdauer
                FROM rechnung r
                JOIN kunde k ON r.kunden_id = k.id
                WHERE r.id = @RechnungsID";

                    MySqlCommand baseCmd = new MySqlCommand(baseQuery, connection);
                    baseCmd.Parameters.AddWithValue("@RechnungsID", rechnungsId);
                    DataTable baseData = new DataTable();
                    baseData.Load(baseCmd.ExecuteReader());

                    // Zimmer-Vorteile laden
                    string featuresQuery = @"
                SELECT DISTINCT
                    e.eigenschaft AS Vorteil,
                    (DATEDIFF(r.ende, r.anfang) + 1) AS Tage,
                    e.preis AS Tagespreis,
                    ((DATEDIFF(r.ende, r.anfang) + 1) * e.preis) AS Gesamt
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

                    // Leistungen laden
                    string servicesQuery = @"
                SELECT 
                    l.leistung AS Leistung,
                    bl.anzahl AS Anzahl,
                    l.preis AS Einzelpreis,
                    (bl.anzahl * l.preis) AS Gesamt
                FROM buchung_hat_leistung bl
                JOIN leistungen l ON bl.leistung_id = l.id
                JOIN buchung b ON bl.buchung_id = b.id
                WHERE b.rechnung_id = @RechnungsID";

                    MySqlCommand servicesCmd = new MySqlCommand(servicesQuery, connection);
                    servicesCmd.Parameters.AddWithValue("@RechnungsID", rechnungsId);
                    DataTable servicesData = new DataTable();
                    servicesData.Load(servicesCmd.ExecuteReader());

                    // Kombinierte DataTable erstellen
                    DataTable combinedData = new DataTable();
                    combinedData.Columns.Add("Typ", typeof(string)); // Typ (Vorteil oder Leistung)
                    combinedData.Columns.Add("Beschreibung", typeof(string)); // Beschreibung (Vorteil oder Leistung)
                    combinedData.Columns.Add("Tage/Anzahl", typeof(int)); // Tage oder Anzahl
                    combinedData.Columns.Add("Einzelpreis", typeof(decimal)); // Preis pro Tag oder Einzelpreis
                    combinedData.Columns.Add("Gesamt", typeof(decimal)); // Gesamtkosten

                    // Zimmer-Vorteile zur kombinierten Tabelle hinzufügen
                    foreach (DataRow row in featuresData.Rows)
                    {
                        combinedData.Rows.Add(
                            "Vorteil",
                            row["Vorteil"],
                            row["Tage"],
                            row["Tagespreis"],
                            row["Gesamt"]
                        );
                        totalGesamt += Convert.ToDecimal(row["Gesamt"]);
                    }

                    // Leistungen zur kombinierten Tabelle hinzufügen
                    foreach (DataRow row in servicesData.Rows)
                    {
                        combinedData.Rows.Add(
                            "Leistung",
                            row["Leistung"],
                            row["Anzahl"],
                            row["Einzelpreis"],
                            row["Gesamt"]
                        );
                        totalGesamt += Convert.ToDecimal(row["Gesamt"]);
                    }

                    // Daten anzeigen
                    if (baseData.Rows.Count > 0)
                    {
                        DataRow baseRow = baseData.Rows[0];
                        rechnungsDetailsTextBlock.Text = $"Kunde: {baseRow["vorname"]} {baseRow["nachname"]}\n"
                                                       + $"Rechnungs-ID: {baseRow["id"]}\n"
                                                       + $"Buchungsdauer: {baseRow["Gesamtdauer"]} Tage\n"
                                                       + $"Zeitraum: {((DateTime)baseRow["anfang"]).ToShortDateString()} - "
                                                       + $"{((DateTime)baseRow["ende"]).ToShortDateString()}";

                        // Gesamtpreis anzeigen
                        gesamtpreisTextBlock.Text = $"Gesamtpreis: {totalGesamt:C}";

                        // Kombinierte Daten anzeigen
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