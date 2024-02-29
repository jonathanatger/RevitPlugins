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


using Verrouiller.ViewModels;


namespace Verrouiller
{
    
    public partial class Ui : Window
    {
        private readonly Document _doc;
        
        private readonly UIDocument _uiDoc;
        private readonly Autodesk.Revit.ApplicationServices.Application _app;        
        List<Objet> listeObjets = new List<Objet>();    

        private EventHandlerWithWpfArg _mExternalMethodWpfArg;

        public bool switchMode = true;

        //Lancer la fenêtre
        public Ui(UIApplication uiApp, EventHandlerWithWpfArg eExternalMethodWpfArg)
        {
            _uiDoc = uiApp.ActiveUIDocument;
            _doc = _uiDoc.Document;
            _app = _doc.Application;            

            Closed += MainWindow_Closed;

            InitializeComponent();

            RemplirListeObjets(_doc);        
              
            _mExternalMethodWpfArg = eExternalMethodWpfArg;
        }


        #region methods 
        //Fermer la fenêtre
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Close();
        }

        //Remplir la liste d'objets à sélectionner
        private void RemplirListeObjets(Document doc)
        {
            List<string> strings = new List<string>()
            {
                "Quadrillages",
                "Niveaux",
                "Topographies",
                "Liens DWG",
                "Liens Revit",
                "Sols"
            };             
           

            List<Objet> objets = new List<Objet>();

            foreach (string st in strings)
            {
                //Créer les objets qui vont être montrés dans l'UI
                objets.Add(new Objet() { Nom = st, Check = false });
            }

            //Montrer les objets dans l'UI
            lbxobjets.ItemsSource = objets;
            

            //Organiser la Objet avec des groupes et trier alphabétiquement
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lbxobjets.ItemsSource);            
            view.SortDescriptions.Add(new SortDescription("Nom", ListSortDirection.Ascending));
            
        }
 

        #endregion methods

        //Sur le clic du bouton, lancer la méthode principale qui crée des feuilles. Celle-ci est appelée par _mExternalMethodWpfArg.Raise, pour être lancée correctement dans un contexte Revit
        private void Btnvalidation_Click_V(object sender, RoutedEventArgs e)
        {
            switchMode = true;
            _mExternalMethodWpfArg.Raise(this);
           
        }

        private void Btnvalidation_Click_D(object sender, RoutedEventArgs e)
        {
            switchMode = false;
            _mExternalMethodWpfArg.Raise(this);

        }


    }
}