﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hotel
{
    /// <summary>
    /// Interaktionslogik für Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        string vorname;
        string nachname;
        DateTime geburtstag;
        int plz;
        string ort;
        string straße_hausnummer;
        DateTime einzugsdatum;

        public Page1()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textbox_vorname.Text) || string.IsNullOrEmpty(textbox_nachname.Text) || geburtstag == null || string.IsNullOrEmpty(textbox_plz.Text) || string.IsNullOrEmpty(textbox_ort.Text) || string.IsNullOrEmpty(textbox_strasse_hausnummer.Text) || einzugsdatum == null)
            {
                vorname = textbox_vorname.Text;
                nachname = textbox_nachname.Text;
                geburtstag = Convert.ToDateTime(datepicker_geburtstag.DisplayDate);
                ort = textbox_ort.Text;
                straße_hausnummer = textbox_strasse_hausnummer.Text;
                einzugsdatum = Convert.ToDateTime(datepicker_einzugsdatum.DisplayDate);
                MessageBox.Show("keine leeren felder möglich");
                return;
            }


            if (!int.TryParse(textbox_plz.Text, out plz) || textbox_plz.Text.Length < 5)
            {
                MessageBox.Show("die Postleitzahl muss eine fünfstellige zahl sein");
            }
            else
            {
                plz = Convert.ToInt32(textbox_plz.Text);
            }


        }
    }
}
