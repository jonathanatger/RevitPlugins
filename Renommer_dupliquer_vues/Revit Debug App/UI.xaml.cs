using System;
using System.Collections.Generic;

using System.ComponentModel;

using System.Linq;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Data;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using ComboBox = System.Windows.Controls.ComboBox;
using Window = System.Windows.Window;


using RenommerDupliquerVues.ViewModels;


namespace RenommerDupliquerVues
{
    /// <summary>
    /// Interaction logic for UI.xaml
    /// </summary> 

    
    public partial class Ui : Window
    {
        private readonly Document _doc;
        
        private readonly UIDocument _uiDoc;
        private readonly Autodesk.Revit.ApplicationServices.Application _app;        
        List<Vue> listeVues = new List<Vue>();    

        private EventHandlerWithWpfArg _mExternalMethodWpfArg;

        public string valeurClassement1;
        public string valeurClassement2;


        //Lancer la fenêtre
        public Ui(UIApplication uiApp, EventHandlerWithWpfArg eExternalMethodWpfArg)
        {
            _uiDoc = uiApp.ActiveUIDocument;
            _doc = _uiDoc.Document;
            _app = _doc.Application;
            this.valeurClassement1 = "01_DOSSIER";
            this.valeurClassement2 = "01_SOUS DOSSIER";

            Closed += MainWindow_Closed;

            InitializeComponent();

            RemplirListeParamVue(_doc);
            RemplirListeGabarits(_doc);
            
              
            _mExternalMethodWpfArg = eExternalMethodWpfArg;
        }



        #region methods 
        //Fermer la fenêtre
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Close();
        }
                

        // Remplir la liste avec tous les paramètres pour trier les vues
        private void RemplirListeParamVue(Document doc)
        {


            Autodesk.Revit.DB.View view = (from element in new FilteredElementCollector(_doc).OfClass(typeof(Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>()
                                           where element.ViewType == ViewType.ThreeD || element.ViewType == ViewType.FloorPlan
                                           || element.ViewType == ViewType.EngineeringPlan || element.ViewType == ViewType.CeilingPlan
                                           || element.ViewType == ViewType.Section || element.ViewType == ViewType.Elevation
                                           select element).ToList().First();

            List<string> liste_guid = new List<string>();
            List<string> l = new List<string>();

            foreach (Parameter para in view.Parameters)
            {
                InternalDefinition def = para.Definition as InternalDefinition;
                if ((para.IsShared && !liste_guid.Contains(para.GUID.ToString()) && para.StorageType == StorageType.String)
                    || (def != null && def.BuiltInParameter == BuiltInParameter.INVALID && !l.Contains(para.Definition.Name) && para.StorageType == StorageType.String))
                {
                    cbxtrivues.Items.Add(para.Definition.Name);
                    cbxtrivues2.Items.Add(para.Definition.Name);

                    if (para.IsShared)
                        liste_guid.Add(para.GUID.ToString());
                    l.Add(para.Definition.Name);

                }
            }

            Parameter echelle = view.get_Parameter(BuiltInParameter.VIEW_SCALE);
            cbxtrivues.Items.Add("Échelle");
            cbxtrivues2.Items.Add("Échelle");

            Parameter type = view.get_Parameter(BuiltInParameter.VIEW_TYPE);
            cbxtrivues.Items.Add(type.Definition.Name);
            cbxtrivues2.Items.Add(type.Definition.Name);

            if (cbxtrivues.Items.Contains("01_DOSSIER")){ 
                cbxtrivues.SelectedItem = "01_DOSSIER";
            } else { cbxtrivues.SelectedItem = "Famille et type"; }

            if (cbxtrivues2.Items.Contains("01_SOUS DOSSIER"))
            {
                cbxtrivues2.SelectedItem = "01_SOUS DOSSIER";
            }
            else { cbxtrivues2.SelectedItem = "Échelle"; }

            

        }

        //Sur un changement de sélection du paramètre de tri de vues
        private void lbxparamvuechange(object sender, SelectionChangedEventArgs e)
        {
            String text = (sender as ComboBox).SelectedItem as String;
            String text2 = valeurClassement2;
            valeurClassement1 = text;
            RemplirListeVues(_doc, text, text2);
        }

        private void lbxparamvuechange2(object sender, SelectionChangedEventArgs e)
        {
            String text = valeurClassement1;
            String text2 = (sender as ComboBox).SelectedItem as String;
            valeurClassement2 = text2;
            RemplirListeVues(_doc, text, text2);
        }

        //Remplir la liste de vues à sélectionner
        private void RemplirListeVues(Document doc, String text, String text2)
        {
            //Obtenir liste vues du projet, en gardant l'Id
            List<ElementId> ensembleVuesId = new List<ElementId>();
            foreach (Autodesk.Revit.DB.View v in new FilteredElementCollector(_doc).OfClass(typeof(Autodesk.Revit.DB.View)))
            {
                if (!(v.IsTemplate)
                && v.ViewType != ViewType.Schedule && !(v.IsAssemblyView)
                && v.ViewType != ViewType.ColumnSchedule
                && v.ViewType != ViewType.Internal
                && v.ViewType != ViewType.ProjectBrowser
                && v.ViewType != ViewType.SystemBrowser
                && v.ViewType != ViewType.PanelSchedule
                && v.ViewType != ViewType.Undefined
                && v.ViewType != ViewType.Legend
                && v.ViewType != ViewType.CostReport
                && v.ViewType != ViewType.LoadsReport
                && v.ViewType != ViewType.Report
                && v.ViewType != ViewType.PresureLossReport
                && v.ViewType != ViewType.DrawingSheet
                && v.ViewType != ViewType.Walkthrough
                    )
                {
                    ensembleVuesId.Add(v.Id);
                }
            }

            List<Vue> vues = new List<Vue>();

            foreach (ElementId id in ensembleVuesId)
            {

                String name = doc.GetElement(id).Name;
                String param;
                String param2;
                String echelle;

                //Name of first property
                try {
                    if (text == "Famille et type")
                    {
                        param = doc.GetElement(id).get_Parameter(BuiltInParameter.VIEW_TYPE).AsValueString();
                    }
                    else if (text == "Échelle")
                    {
                        param = doc.GetElement(id).get_Parameter(BuiltInParameter.VIEW_SCALE_PULLDOWN_METRIC).AsValueString();
                    }
                    else if (text == "Niveau associé")
                    {
                        param = doc.GetElement(id).get_Parameter(BuiltInParameter.PLAN_VIEW_LEVEL).AsValueString();
                    }
                    else
                    {
                        param = doc.GetElement(id).LookupParameter(text).AsString();
                    }

                    if (param == "" || param == null) { param = "???"; };

                }
                catch { param = "???"; };

                // Name of second property
                try
                {
                    if (text2 == "Famille et type")
                    {
                        param2 = doc.GetElement(id).get_Parameter(BuiltInParameter.VIEW_TYPE).AsValueString();
                    }
                    else if (text2 == "Échelle")
                    {
                        param2 = doc.GetElement(id).get_Parameter(BuiltInParameter.VIEW_SCALE_PULLDOWN_METRIC).AsValueString();
                    }
                    else if (text2 == "Niveau associé")
                    {
                        param2 = doc.GetElement(id).get_Parameter(BuiltInParameter.PLAN_VIEW_LEVEL).AsValueString();
                    }
                    else
                    {
                        param2 = doc.GetElement(id).LookupParameter(text2).AsString();
                    }

                    if (param2 == "" || param2 == null) { param2 = "???"; };

                }
                catch { param2 = "???"; };



                echelle = doc.GetElement(id).get_Parameter(BuiltInParameter.VIEW_SCALE_PULLDOWN_METRIC).AsValueString();

                //Créer les objets qui vont être montrés dans l'UI
                vues.Add(new Vue() { Nom = name, Check = false, Prop = param, Prop2 = param2, Echelle = echelle, Id = id });
            }

            //Montrer les objets dans l'UI
            lbxvues.ItemsSource = vues;

            

            //Organiser la vue avec des groupes et trier alphabétiquement

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lbxvues.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Prop");
            view.GroupDescriptions.Add(groupDescription);
            PropertyGroupDescription groupDescription2 = new PropertyGroupDescription("Prop2");
            view.GroupDescriptions.Add(groupDescription2);
            PropertyGroupDescription groupDescription3 = new PropertyGroupDescription("Echelle");
            view.GroupDescriptions.Add(groupDescription3);

            view.SortDescriptions.Add(new SortDescription("Prop", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("Prop2", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("Echelle", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("Nom", ListSortDirection.Ascending));

            
        }

        //Obtenir tout les Gabarits de vue dans l'UI
        private void RemplirListeGabarits(Document doc)
        {
            
            List<Vue> gabarits = new List<Vue>();

            foreach (View v in new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.View)).Cast<View>())
            {
                if(v.IsTemplate == true)
                {
                    gabarits.Add(new Vue() { Nom = v.Name, Id=v.Id});                    
                }
            }

            //Montrer dans l'UI
            cbxgabarit.ItemsSource = gabarits;
            
            //Trier alphabétiquement    
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(cbxgabarit.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Nom");
            view.GroupDescriptions.Add(groupDescription);            
            view.SortDescriptions.Add(new SortDescription("Nom", ListSortDirection.Ascending));

        }

        #endregion methods



        private void lvw_nomfeuille_majtexte(object sender, SelectionChangedEventArgs args)
        {
            try
            {
                List<string> resultats = ModificationNom(0);
                anciennom.Text = resultats[0];
                nouveaunom.Text = resultats[1];
            }
            catch { }
        }

        private void tbx_nomfeuille_majtexte (object sender, TextChangedEventArgs args)
        {          
            try
            {
                List<string> resultats = ModificationNom(0);
                anciennom.Text = resultats[0];
                nouveaunom.Text = resultats[1];
            }
            catch { }
        }

        private void rbs_nomfeuille_checked (object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> resultats = ModificationNom(0);
                anciennom.Text = resultats[0];
                nouveaunom.Text = resultats[1];
            }
            catch {  }
        }

        public List<string> ModificationNom (int i)
        {
            Document doc = _doc;
            Vue vue;
            string ancienlabel = "";
            string nouveaulabel = "";
            List<string> retour = new List<string>();

            List<string> viewlist_global = new List<string>();
            List<string> vuescreees = new List<string>();
            List<string> vuesnoncreees = new List<string>();            
            View view;
            int choix = 0;

            //Prendre dans l'interface le choix du mode de renommage
            if (insertion.IsChecked == true)
            {
                choix = 0;
            }
            else if (remplacement.IsChecked == true)
            {
                choix = 1;
            }
            else if (suppression.IsChecked == true)
            {
                choix = 2;
            }

            //Méthodes de modification
            if(lbxvues.SelectedItems.Count >= i) 
            { 
                vue = lbxvues.SelectedItems[i] as Vue;
                view = doc.GetElement(vue.Id) as View;
                ancienlabel = view.Name;

                if (choix == 0)
                {
                    string textinsere = texteinsere.Text;
                    int y;
                    bool check;
                    
                    check = Int32.TryParse(positiontexteinsere.Text, out y);

                    if (check == false)
                    { y = 0; }
                    if (y > ancienlabel.Length)
                    {
                        y = ancienlabel.Length;
                    }
                    //nouveau nom
                    nouveaulabel = view.Name.Substring(0, y) + textinsere + view.Name.Substring(y);

                    //check pour caractères interdits
                    if (nouveaulabel.Contains("{") || nouveaulabel.Contains("}"))
                    {
                        nouveaulabel.Replace("{", "(").Replace("}", ")");
                    }


                }

                //remplacement
                else if (choix == 1)
                {
                    string textearemplacer = texterecherche.Text;
                    string textederemplacement = textepourremplacer.Text;

                    if(!(textearemplacer == ""))
                    {
                        nouveaulabel = view.Name.Replace(textearemplacer, textederemplacement);
                    }
                    else
                    {
                        nouveaulabel = view.Name;
                    }

                    nouveaulabel.Replace("{", "(").Replace("}", ")");

                }

                //suppression
                else if (choix == 2)
                {
                    string nbre = nombrecaracteressupprimes.Text;
                    string pos = positiontextesupprime.Text;

                    int x;
                    int y;
                    bool check;
                    bool check2;
                    check = Int32.TryParse(nombrecaracteressupprimes.Text, out x);
                    check2 = Int32.TryParse(positiontextesupprime.Text, out y);

                    if (check == false)
                    { x = 0; }

                    if (check2 == false)
                    { y = 0; }

                    if (ancienlabel.Length > x && ancienlabel.Length > x + y)
                    {
                        nouveaulabel = view.Name.Substring(0, y) + view.Name.Substring(y + x);
                    }
                    else if (ancienlabel.Length <= y)
                    {
                        nouveaulabel = view.Name;
                    }
                    else if (ancienlabel.Length > y && ancienlabel.Length <= x + y && ancienlabel.Length > x)
                    {
                        nouveaulabel = view.Name.Substring(0, y);
                    }
                    else if (ancienlabel.Length <= x && y != 0)
                    {
                        nouveaulabel = nouveaulabel = view.Name.Substring(0, y);
                    }
                    else if (ancienlabel.Length <= x)
                    {
                        nouveaulabel = "Nom supprimé (" + view.Name + ")";
                    }

                    nouveaulabel.Replace("{", "(").Replace("}", ")");

                }
                retour.Add(ancienlabel);
                retour.Add(nouveaulabel);
            }
            else 
            {
                ancienlabel = "";
                nouveaulabel = "";
                retour.Add(ancienlabel);
                retour.Add(nouveaulabel);            
            }
            
            return retour;
        }
    

        //Sur le clic du bouton, lancer la méthode principale qui crée des feuilles. Celle-ci est appelée par _mExternalMethodWpfArg.Raise, pour être lancée correctement dans un contexte Revit
        private void Btnvalidation_Click(object sender, RoutedEventArgs e)
        {
            _mExternalMethodWpfArg.Raise(this);
           
        }

        
    }
}