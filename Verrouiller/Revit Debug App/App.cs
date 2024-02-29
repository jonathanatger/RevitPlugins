﻿#region Namespaces

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media.Imaging;

#endregion

namespace Verrouiller
{

    // [Guid("B359C53B-96E7-49D2-91DC-62AB047DE96F")]

    class App : IExternalApplication
    {
        // class instance
        public static App ThisApp = null;

        // ModelessForm instance
        private Ui _mMyForm;

        public Result OnStartup(UIControlledApplication a)
        {
            _mMyForm = null; // no dialog needed yet; the command will bring it
            ThisApp = this; // static access to this application instance

            // Method to add Tab and Panel 
            RibbonPanel panel = RibbonPanel(a);
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButton button = panel.AddItem( new PushButtonData("Verrouiller/Déverouiller", "Verrouiller/Déverouiller", thisAssemblyPath, "Verrouiller.EntryCommand")) as PushButton;

            button.ToolTip = "Verrouiller et déverouiller quadrillages, niveaux et autres objets";
            Uri uriImage = new Uri("pack://application:,,,/Verrouiller;component/Resources/Verrouiller_32.png");
            BitmapImage largeImage = new BitmapImage(uriImage);
            button.LargeImage = largeImage;

            a.ApplicationClosing += a_ApplicationClosing; //Set Application to Idling
            a.Idling += a_Idling;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

        //   The external command invokes this on the end-user's request
        public void ShowForm(UIApplication uiapp)
        {
            // If we do not have a dialog yet, create and show it
            if (_mMyForm == null || _mMyForm != null) // || m_MyForm.IsDisposed
            {
                //EXTERNAL EVENTS WITH ARGUMENTS
                
                EventHandlerWithWpfArg eDatabaseStore = new EventHandlerWithWpfArg();

                // The dialog becomes the owner responsible for disposing the objects given to it.
                _mMyForm = new Ui(uiapp, eDatabaseStore);
                _mMyForm.Show();
            }
        }

        #region Idling & Closing

        //*****************************a_Idling()*****************************
        void a_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
        }

        //*****************************a_ApplicationClosing()*****************************
        void a_ApplicationClosing(object sender, Autodesk.Revit.UI.Events.ApplicationClosingEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
            System.Environment.Exit(1);
        }

        #endregion

        #region Ribbon Panel

        public RibbonPanel RibbonPanel(UIControlledApplication a)
        {
            string tab = "Archi5"; // Tab name
            // Empty ribbon panel 
            RibbonPanel ribbonPanel = null;
            // Try to create ribbon tab. 
            try
            {
                a.CreateRibbonTab(tab);
            }
            catch
            {
            }

            // Try to create ribbon panel.
            try
            {
                RibbonPanel panel = a.CreateRibbonPanel(tab, "Modélisation");
            }
            catch
            {
            }

            // Search existing tab for your panel.
            List<RibbonPanel> panels = a.GetRibbonPanels(tab);
            foreach (RibbonPanel p in panels)
            {
                if (p.Name == "Modélisation")
                {
                    ribbonPanel = p;
                }
            }

            //return panel 
            return ribbonPanel;
        }

        #endregion
    }


    #region Method-Specific External Events

   

    public class EventHandlerWithWpfArg : RevitEventWrapper<Ui>
    {
        public override void Execute(UIApplication uiApp, Ui ui)
        {
            // SETUP
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // METHODS
            //  au moment ou le bouton est cliqué : analyse le contenu des paramètres et si il matche, va lancer une copie 
            
            Methods.ModificationElements(ui, doc, ui.switchMode);
                       
        }
    }

    #endregion
}