using System;
using System.Windows;
using Autodesk.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Window = System.Windows.Window;

using Autodesk.Revit.UI.Selection;
using System.Windows.Interop;
using System.Diagnostics;
using System.Windows.Controls;
using SynchroniserPresentations.ViewModels;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SynchroniserPresentations
{
    /// <summary>
    /// Interaction logic for UI.xaml
    /// </summary> 



    
    
    public partial class Ui : Window
    {
        //Handling the window and giving the ability to simulate an ESC key press
        public static IntPtr RevitWindowPtr { [DebuggerStepThrough] get => ComponentManager.ApplicationWindow; }

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);


        //Variables
        private readonly Document _doc;        
        private readonly UIDocument _uiDoc;
        private readonly Autodesk.Revit.ApplicationServices.Application _app;  
        private EventHandlerWithWpfArg _mExternalMethodWpfArg;


        private string viewname;
        private string sheetname;

        public ElementId referenceViewportId;
        public ElementId selectedViewportId;
        public double XRefTitleBlock;
        public double YRefTitleBlock;


        //filter class to help select the right elementtype
        private class MassSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Viewports)
                {
                    return true;
                }
                return false;
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }

        //designed to help keep app in focus
        public class WindowHandleHelper : IWin32Window
        {
            IntPtr _hwnd;

            public WindowHandleHelper(IntPtr h)
            {
                _hwnd = h;
            }

            public IntPtr Handle
            {
                get
                {
                    return _hwnd;
                }
            }
        }

        //Lancer la fenêtre
        public Ui(UIApplication uiApp, EventHandlerWithWpfArg eExternalMethodWpfArg)
        {
            _uiDoc = uiApp.ActiveUIDocument;
            _doc = _uiDoc.Document;
            _app = _doc.Application;        
            
            Closed += MainWindow_Closed;

            InitializeComponent();


            //code pour faire en sorte de garder l'application au premier plan
            //Obtenir le processus en cours et son interface utilisateur
            Process revitProcess = Process.GetCurrentProcess();            
            IntPtr revitMainWindowPointer = revitProcess.MainWindowHandle;

            // faire de l'application un 'enfant' de la fenêtre principale
            WindowHandleHelper revit_window = new WindowHandleHelper(revitMainWindowPointer);
            WindowInteropHelper helper = new WindowInteropHelper(this);
            helper.Owner = revit_window.Handle;

            
            btnsynchrovues.IsEnabled = false;
            btndeselection.IsEnabled = false;

           

            _mExternalMethodWpfArg = eExternalMethodWpfArg;


        }

        #region methods 
        //Fermer la fenêtre
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Close();
        }




        #endregion methods


        //Sur le clic du bouton, lancer la méthode principale qui crée des feuilles. Celle-ci est appelée par _mExternalMethodWpfArg.Raise, pour être lancée correctement dans un contexte Revit
        private void Btnselection_Click(object sender, RoutedEventArgs e)
        {
            
            try {
                btnselectionvue.IsEnabled = false;
                btndeselection.IsEnabled = true;

                //organiser la sélection d'une vue sur une feuille            
                ISelectionFilter selFilter = new MassSelectionFilter();
                Reference pickedViews = _uiDoc.Selection.PickObject(ObjectType.Element, selFilter, "Selectionner une vue sur une feuille");                              

                //Obtenir les infos liées aux élements
                referenceViewportId = pickedViews.ElementId;
                Viewport vp = _doc.GetElement(referenceViewportId) as Viewport;
                ViewSheet sh = _doc.GetElement(vp.SheetId) as ViewSheet;
                Element refTitleBlock = new FilteredElementCollector(_doc, vp.SheetId).OfCategory(BuiltInCategory.OST_TitleBlocks).FirstElement();
                                
                viewname = _doc.GetElement(vp.ViewId).Name;
                sheetname = sh.Name;
                XRefTitleBlock = refTitleBlock.get_BoundingBox(sh).Min.X;
                YRefTitleBlock = refTitleBlock.get_BoundingBox(sh).Max.Y;

                //mettre à jour l'interface et la ramener au devant de l'écran
                txtnomvue.Text = viewname;
                txtnomfeuille.Text = sheetname;

                btnsynchrovues.IsEnabled = true;
                btndeselection.IsEnabled = true;

                btnselectionvue.IsEnabled = false;             

                this.Focus();
            }
            catch 
            {
                btndeselection.IsEnabled = true;
                this.Focus(); 
            }

        }

        //Sur le clic du bouton, 
        private void Btndeselection_Click(object sender, RoutedEventArgs e)
        {
            //Simuler la touche ESC, pour sortir de la fonctionnalité de sélection d'objets de Revit si elle est lancée
            SetForegroundWindow(RevitWindowPtr);
            keybd_event(0x1B, 0, 0, 0);
            keybd_event(0x1B, 0, 2, 0);

            //remettre l'interface à zéro
            btnsynchrovues.IsEnabled = false;
            btndeselection.IsEnabled = false;

            btnselectionvue.IsEnabled = true;            

            txtnomvue.Text = "";
            txtnomfeuille.Text = "";           

        }

        //Sur le clic du bouton, lancer la méthode principale qui crée des feuilles. Celle-ci est appelée par _mExternalMethodWpfArg.Raise, pour être lancée correctement dans un contexte Revit
        private void Btnvalidation_Click(object sender, RoutedEventArgs e)
        {            

            try {

                ISelectionFilter selFilter2 = new MassSelectionFilter();
                Reference pickedSelView = _uiDoc.Selection.PickObject(ObjectType.Element, selFilter2, "Selectionner une vue sur une feuille");
                selectedViewportId = pickedSelView.ElementId;

                /*
                //Obtenir les infos liées aux élements
                referenceViewportId = pickedSelView.ElementId;

                IList<ElementId> selectedIds = _uiDoc.Selection.GetElementIds() as IList<ElementId>;
                Viewport vp2 = _doc.GetElement(selectedIds[0]) as Viewport;
                ElementId vId = vp2.ViewId;
                */                
                
                _mExternalMethodWpfArg.Raise(this);

            }
            catch { this.Focus(); }

            

        }

        
    }
}