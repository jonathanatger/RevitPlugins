using System;
using System.Collections.Generic;

using System.ComponentModel;

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using ComboBox = System.Windows.Controls.ComboBox;
using Window = System.Windows.Window;


using CreationMultiplesFeuilles.ViewModels;


namespace CreationMultiplesFeuilles
{
    /// <summary>
    /// Interaction logic for UI.xaml
    /// </summary> 

    
    public partial class Ui : Window
    {
        private readonly Document _doc;
        //private readonly UIApplication _uiApp;
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
            
              
            _mExternalMethodWpfArg = eExternalMethodWpfArg;


           
            
            RemplirListeParamVue(_doc);
            RemplirListeCartouches(_doc);
            RemplirListeFenetres(_doc);
            RemplirListeClassement(_doc);


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
                    boxtrivues.Items.Add(para.Definition.Name);
                    boxtrivues2.Items.Add(para.Definition.Name);
                    if (para.IsShared)
                        liste_guid.Add(para.GUID.ToString());
                    l.Add(para.Definition.Name);

                }
            }

            Parameter echelle = view.get_Parameter(BuiltInParameter.VIEW_SCALE);
            boxtrivues.Items.Add("Échelle");
            boxtrivues2.Items.Add("Échelle");

            Parameter type = view.get_Parameter(BuiltInParameter.VIEW_TYPE);
            boxtrivues.Items.Add(type.Definition.Name);
            boxtrivues2.Items.Add(type.Definition.Name);

            

            try
            {
                boxtrivues.SelectedItem = "01_DOSSIER";
            }
            catch
            {
                boxtrivues.SelectedItem = "Famille et type";
            }

            try
            {
                boxtrivues2.SelectedItem = "01_SOUS DOSSIER";
            }
            catch
            {
                boxtrivues2.SelectedItem = "Échelle";
            }

            if (boxtrivues.Items.Contains("01_DOSSIER"))
            {
                boxtrivues.SelectedItem = "01_DOSSIER";
            }
            else { boxtrivues.SelectedItem = "Famille et type"; }

            if (boxtrivues2.Items.Contains("01_SOUS DOSSIER"))
            {
                boxtrivues2.SelectedItem = "01_SOUS DOSSIER";
            }
            else { boxtrivues2.SelectedItem = "Échelle"; }


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
            Autodesk.Revit.DB.ViewSheet sheet;

            //Obtenir feuille
            try 
            {
                sheet = (from element in new FilteredElementCollector(_doc).OfClass(typeof(Autodesk.Revit.DB.ViewSheet)).Cast<Autodesk.Revit.DB.ViewSheet>()
                                                     select element).ToList().First();
            }
            catch
            {
                sheet = null;
                    
            }
            
            if(sheet == null)
            {
                TaskDialog td = new TaskDialog("Avertissement");

                td.TitleAutoPrefix = true;

                td.Title = "Avertissement";

                td.MainContent = "Il faut créer une première feuille pour pouvoir commencer.";
                td.CommonButtons = TaskDialogCommonButtons.Ok;
                TaskDialogResult result = td.Show();
                return;
            }
            else
            {
                //Obtenir liste vues
                List<ElementId> ensembleVuesId = new List<ElementId>();
                foreach (Autodesk.Revit.DB.View v in new FilteredElementCollector(_doc).OfClass(typeof(Autodesk.Revit.DB.View)))
                {
                    Parameter test = v.get_Parameter(BuiltInParameter.VIEWER_DETAIL_NUMBER);
                    if (test != null && test.AsString() != "" && Viewport.CanAddViewToSheet(doc, sheet.Id, v.Id) == true)
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

                }
                List<Vue> vues = new List<Vue>();

                foreach (ElementId id in ensembleVuesId)
                {

                    String name = doc.GetElement(id).Name;
                    String param;
                    String param2;
                    String echelle;

                    try
                    {
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

                    vues.Add(new Vue() { Nom = name, Check = false, Prop = param, Prop2 = param2, Echelle = echelle, Id = id });

                }

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


                if (ensembleVuesId.Count() == 0)
                {
                    TaskDialog td = new TaskDialog("Avertissement");

                    td.TitleAutoPrefix = true;

                    td.Title = "Avertissement";

                    td.MainContent = "Toutes les vues du modèle sont déjà placées sur des feuilles.";
                    td.CommonButtons = TaskDialogCommonButtons.Ok;
                    TaskDialogResult result = td.Show();
                    return;
                }

            }

            

            
           

            
        }

        //Obtenir tout les cartouches dans l'UI
        private void RemplirListeCartouches(Document doc)
        {
            List<FamilySymbol> familyList = (from elem in new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_TitleBlocks)
                                             let type = elem as FamilySymbol
                                             select type).ToList();

            List<Vue> listecartouches = new List<Vue>();

            foreach (FamilySymbol fs in familyList)
            {
                listecartouches.Add(new Vue() {Nom= fs.Family.Name + " : " + fs.Name, Id=fs.Id}  );
                
            }
            lbxcartouche.ItemsSource = listecartouches;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lbxcartouche.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Nom", ListSortDirection.Ascending));

            if (lbxcartouche.Items.Count > 0)
            {
                lbxcartouche.SelectedIndex = 0;
            } else
            {
                listecartouches.Add(new Vue() { Nom = "Aucun" });
                
            }

            


        }

        //Faire apparaitre les différents types de fenetres dans l'UI
        private void RemplirListeFenetres(Document doc)
        {
            List<ElementType> viewportTypes = new FilteredElementCollector(doc).OfClass(typeof(ElementType)).Cast<ElementType>().Where(q => (q.FamilyName == "Fenêtre" || q.FamilyName == "Viewport")).ToList();

            List<Vue> listefenetres = new List<Vue>();

            foreach (ElementType et in viewportTypes)
            {
                listefenetres.Add(new Vue() { Nom = et.Name});
                
            }

            lbxfenetre.ItemsSource = listefenetres;

            if (lbxfenetre.Items.Count > 0)
            {
                lbxfenetre.SelectedIndex = 0;
            }

        }

        //Obtenir toutes les options existantes pour les paramètres de classement des feuilles et les intégrer aux listes déroulantes
        private void RemplirListeClassement(Document doc)
        {
            List<ViewSheet> sheets = (from element in new FilteredElementCollector(_doc).OfClass(typeof(Autodesk.Revit.DB.ViewSheet)).Cast<Autodesk.Revit.DB.ViewSheet>()
                                                 select element).ToList();

            List<string> listeclassement1 = new List<string>();
            List<string> listeclassement2 = new List<string>();
            try
            {
                foreach (ViewSheet sh in sheets)
                {
                    string param1 = sh.LookupParameter(valeurClassement1).AsString();
                    string param2 = sh.LookupParameter(valeurClassement2).AsString();


                    if (!listeclassement1.Contains(param1))
                    {
                        listeclassement1.Add(param1);
                    }

                    if (!listeclassement2.Contains(param2))
                    {
                        listeclassement2.Add(param2);
                    }

                }
            }
            catch
            {
                listeclassement1.Add("");
                listeclassement2.Add("");
            }
            

            listeclassement1.Sort();
            listeclassement2.Sort();
            

            foreach (string s in listeclassement1)
            {
                lbxclassement1.Items.Add(s);
            }
            foreach (string s in listeclassement2)
            {
                lbxclassement2.Items.Add(s);
            }
            
            
        }


        #endregion methods




        //Sur le clic du bouton, lancer la méthode principale qui crée des feuilles. Celle-ci est appelée par _mExternalMethodWpfArg.Raise, pour être lancée correctement dans un contexte Revit
        private void Btnvalidation_Click(object sender, RoutedEventArgs e)
        {
            _mExternalMethodWpfArg.Raise(this);
           
        }

        
    }
}