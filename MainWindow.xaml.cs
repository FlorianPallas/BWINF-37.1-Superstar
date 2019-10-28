using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Superstar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

        private Beziehung[] BeziehungsDaten = Array.Empty<Beziehung>(); // Kann nur durch Anfragen abgerufen werden

        private string[] Mitglieder = Array.Empty<string>(); // Liste aller Gruppenmitglieder
        private List<Beziehung> Beziehungen = new List<Beziehung>(); // Liste bereits ausgeführter Anfragen (Beziehungen)
        private List<string> Anfragen = new List<string>(); // Liste bereits ausgeführter Anfragen (String)
        private string Dateiname = String.Empty;
        private string Superstar = String.Empty;

        // =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

        private bool FindeSuperstar(out string Superstar)
        {
            // Vorherige Anfragen + Beziehungen löschen
            Anfragen.Clear();
            Beziehungen.Clear();

            foreach (string Mitglied in Mitglieder)
            {
                // Superstar ist nur wer keinem folgt und trotzdem jeden als Follower hat
                if (AFolgtNiemanden(Mitglied) && JederFolgtA(Mitglied))
                {
                    // Superstar
                    Superstar = Mitglied;
                    return true;
                }
            }

            Superstar = String.Empty;
            return false;
        }

        private bool AFolgtNiemanden(string A)
        {
            foreach (string Mitglied in Mitglieder)
            {
                // A überspringen
                if (Mitglied == A)
                {
                    continue;
                }

                // A scheidet aus falls eine der Anfragen positiv ist
                if (AFolgtB_Anfrage(A, Mitglied))
                {
                    return false;
                }
            }

            return true;
        }

        private bool JederFolgtA(string A)
        {
            foreach (string Mitglied in Mitglieder)
            {
                // A überspringen
                if (Mitglied == A)
                {
                    continue;
                }

                // A scheidet aus falls eine der Anfragen negativ ist
                if (!AFolgtB_Anfrage(Mitglied, A))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AFolgtB_Anfrage(string A, string B)
        {
            // Gespeicherte Beziehungen durchsuchen
            Beziehung BezGespeichert = Beziehungen.Find(Bez => Bez.A == A && Bez.B == B);

            if (BezGespeichert != null && BezGespeichert.Folgt == true)
            {
                return true;
            }
            else
            {
                // Neue Beziehung abspeichern
                bool AntwortNeu = AFolgtB_AnfrageNeu(A, B);
                Beziehung BezNeu = new Beziehung()
                {
                    A = A,
                    B = B,
                    Folgt = AntwortNeu
                };
                Beziehungen.Add(BezNeu);

                if (AntwortNeu)
                {
                    Anfragen.Add("Folgt " + A + " -> " + B + " ? => JA");
                    return true;
                }
                else
                {
                    Anfragen.Add("Folgt " + A + " -> " + B + " ? => NEIN");
                    return false;
                }
            }
        }

        private bool AFolgtB_AnfrageNeu(string A, string B)
        {
            // Finde passende Beziehung (aus Datei)
            Beziehung[] PassendeBeziehungen = BeziehungsDaten.Where(Bez => Bez.A == A && Bez.B == B).ToArray();

            // Ist kein Eintrag vorhanden so wir false zurückgegeben
            if (PassendeBeziehungen.Length == 0)
            {
                return false;
            }

            // Nur erst Beziehung (nur 1 erwartet) berücksichtigen
            return true;
        }

        // =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

        private bool OeffneDatei(out string[] Mitglieder, out Beziehung[] BeziehungsDaten)
        {
            // Zeige Dateidialog
            OpenFileDialog Dialog = new OpenFileDialog()
            {
                Filter = "Textdateien|*.txt",
                Title = "Gruppendatei auswählen ..."
            };

            // Ergebnis überprüfen
            if (Dialog.ShowDialog() != true)
            {
                Mitglieder = Array.Empty<string>();
                BeziehungsDaten = Array.Empty<Beziehung>();
                return false;
            }

            // Datei überprüfen
            if (!File.Exists(Dialog.FileName))
            {
                Mitglieder = Array.Empty<string>();
                BeziehungsDaten = Array.Empty<Beziehung>();
                return false;
            }

            // Datei einlesen
            string[] Datei = File.ReadAllLines(Dialog.FileName);

            // Leere Zeile entfernen
            if(Datei[Datei.Length - 1] == String.Empty)
            {
                Array.Resize(ref Datei, Datei.Length - 1);
            }

            // Mitglieder sammeln
            Mitglieder = Datei[0].Split(' ');

            // Beziehungen sammeln
            BeziehungsDaten = new Beziehung[Datei.Length - 1];
            for (int i = 0; i < Datei.Length -1; i++)
            {
                BeziehungsDaten[i] = new Beziehung()
                {
                    Folgt = true,
                    A = Datei[i + 1].Split(' ')[0],
                    B = Datei[i + 1].Split(' ')[1]
                };
            }

            // Setze Dateiname
            Dateiname = System.IO.Path.GetFileName(Dialog.FileName);

            // Lade Listen
            MitgliederLaden();
            DetailsLaden();

            return true;
        }

        private void MitgliederLaden()
        {
            ListViewGruppe.Items.Clear();

            foreach(var Mitglied in Mitglieder)
            {
                ListViewGruppe.Items.Add(Mitglied);
            }

            GroupBoxGruppe.Header = "Gruppe (" + Mitglieder.Length + " Mitglieder)";
        }

        private void AnfragenLaden()
        {
            ListViewAnfragen.Items.Clear();

            foreach (string Anfrage in Anfragen)
            {
                ListViewAnfragen.Items.Add(Anfrage);
            }

            GroupBoxAnfragen.Header = "Anfragen (" + Anfragen.Count + ")";
        }

        private void DetailsLaden()
        {
            LabelDateiname.Content = Dateiname;
            LabelMitglieder.Content = Mitglieder.Length.ToString();
            LabelBeziehungen.Content = BeziehungsDaten.Length.ToString();
            LabelAnfragen.Content = Anfragen.Count.ToString();
            LabelSuperstar.Content = Superstar;

            if(Anfragen.Count == 0)
            {
                LabelAnfragen.Content = "-";
            }

            if(Superstar == String.Empty)
            {
                LabelSuperstar.Content = "?";
            }
        }

        // =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

        private void ButtonFindeSuperstar_Click(object sender, RoutedEventArgs e)
        {
            // Superstar finden
            string Ergebnis;
            if (FindeSuperstar(out Ergebnis))
            {
                Superstar = Ergebnis;
            }
            else
            {
                Superstar = "Niemand";
            }

            // Listen laden
            MitgliederLaden();
            AnfragenLaden();
            DetailsLaden();
        }

        private void ButtonOeffneDatei_Click(object sender, RoutedEventArgs e)
        {
            // Mitglieder und Beziehungen auslesen
            if (!OeffneDatei(out Mitglieder, out BeziehungsDaten))
            {
                return;
            }

            Anfragen.Clear();

            // Elemente freischalten
            ButtonFindeSuperstar.IsEnabled = true;
            GroupBoxDetails.IsEnabled = true;
            GroupBoxAnfragen.IsEnabled = true;
            GroupBoxGruppe.IsEnabled = true;
            ButtonOeffne.IsEnabled = true;

            Superstar = String.Empty;
            AnfragenLaden();
            DetailsLaden();
        }

        private void ButtonBeenden_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

        private class Beziehung
        {
            public bool Folgt = false;
            public string A = String.Empty;
            public string B = String.Empty;
        }
    }
}
