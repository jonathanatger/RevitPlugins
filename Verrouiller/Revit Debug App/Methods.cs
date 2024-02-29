using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Verrouiller.ViewModels;

namespace Verrouiller
{

    /// <summary>
    /// Create methods here that need to be wrapped in a valid Revit Api context.
    /// Things like transactions modifying Revit Elements, etc.
    /// </summary>
    internal class Methods
    {

        public static void ModificationElements(Ui ui, Document doc, bool b)
        {

            //liste objet View Revit
            
            List<string> listeChoix = new List<string>();
            List<string> listeMessage = new List<string>();

            //Lancer la transaction Revit
            using (Transaction t = new Transaction(doc, "Verrouiller/Déverouiller"))
            {
                t.Start("Verrouiller/Déverouiller");
                                
                //Obtenir liste vues
                listeChoix = GetListObjects(ui, doc);

                listeMessage = VerrouillerObjets(listeChoix, doc, b);

                
                t.Commit();

                //Terminer la transaction               
                t.Dispose();
            }

            ui.txtbox.Text = listeMessage.Count.ToString();

            //Message de validation
            TaskDialog td = new TaskDialog("Confirmation");
            td.AllowCancellation = true;
            td.TitleAutoPrefix = true;
            if (b)
            {
                td.Title = "Verrouiller les éléments";
                if (listeMessage.Count != 0)
                {
                    td.MainInstruction = "Les éléments suivants ont été verrouillés :";
                    string final = string.Join(Environment.NewLine, listeMessage.ToArray());
                    td.MainContent = final;
                }
                else
                {
                    td.MainInstruction = "Il n'y a aucun élément restant à verrouiller.";
                }
            } 
            else
            {
                td.Title = "Déverrouiller les éléments";
                if (listeMessage.Count != 0)
                {
                    td.MainInstruction = "Les éléments suivants ont été déverrouillés :";
                    string final = string.Join(Environment.NewLine, listeMessage.ToArray());
                    td.MainContent = final;
                }
                else
                {
                    td.MainInstruction = "Il n'y a aucun élément restant à déverrouiller.";
                }
            }                                 

            td.CommonButtons = TaskDialogCommonButtons.Ok;
            TaskDialogResult result = td.Show();

            ui.Focus();
        }
        

        //Obtenir les noms des objets choisis
        public static List<string> GetListObjects(Ui ui, Document doc)
        {
            
            List<string> vliste_3 = new List<string>();

            List<Objet> liste = new List<Objet>();

            foreach(object v in ui.lbxobjets.SelectedItems)
            {
                Objet obj = v as Objet;
                vliste_3.Add(obj.Nom);
            }          
            
            return vliste_3;
        }
        
        // a partir des noms des types d'objets, verrouille les objets
        public static List<string> VerrouillerObjets(List<string> choix, Document doc, bool b)
        {
            List<string> liste_cat = choix;
            List<string> liste = new List<string>();
            int nb_grids = 0;
            int nb_levels = 0;
            int nb_cao = 0;
            int nb_links = 0;
            int nb_topo = 0;
            int nb_sols = 0;
            string nomCao;

            if (liste_cat.Count == 0)
            {
                liste.Add("Il n'y a aucun élément à verrouiller.");
            }

            if (liste_cat.Contains("Quadrillages"))
            {
                foreach (Grid grid in new FilteredElementCollector(doc).OfClass(typeof(Grid)).Cast<Grid>())
                {
                    if (grid.Pinned == !b)
                    {
                        try
                        {
                            grid.Pinned = b;
                            nb_grids += 1;
                        }
                        catch
                        {
                        }
                    }                    
                }
                if (nb_grids > 1)
                    liste.Add(nb_grids.ToString() + " quadrillages");
                if (nb_grids == 1)
                    liste.Add("1 quadrillage");

            }

            if (liste_cat.Contains("Niveaux"))
            {
                foreach (Level lv in new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>())
                {
                    if (lv.Pinned == !b)
                    {
                        lv.Pinned = b;
                        nb_levels += 1;
                    }
                    
                }
                if (nb_levels > 1)
                    liste.Add(nb_levels.ToString() + " niveaux");
                if (nb_levels == 1)
                    liste.Add("1 niveau");
            }

            if (liste_cat.Contains("Liens Revit"))
            {
                foreach (RevitLinkInstance link in new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>())
                {
                    RevitLinkType type = doc.GetElement(link.GetTypeId()) as RevitLinkType;
                    if (!type.IsNestedLink)
                    {
                        if (link.Pinned == !b)
                        {
                            link.Pinned = b;
                            nb_links += 1;
                        }                        
                    }
                }
                if (nb_links > 1)
                    liste.Add(nb_links.ToString() + " liens RVT");
                if (nb_links == 1)
                    liste.Add("1 lien RVT");
            }

            if (liste_cat.Contains("Liens DWG"))
            {
                foreach (ImportInstance ii in new FilteredElementCollector(doc).OfClass(typeof(ImportInstance)).Cast<ImportInstance>())
                {
                    if (ii.Pinned == !b)
                    {
                        ii.Pinned = b;
                        nomCao = ii.LookupParameter("Nom").AsString();
                        if (nomCao.EndsWith(".dwg"))
                        {
                            nb_cao += 1;
                        }
                    }
                    
                }
                if (nb_cao > 1)
                    liste.Add(nb_cao.ToString() + " liens DWG");
                if (nb_cao == 1)
                    liste.Add("1 lien DWG");
            }

            if (liste_cat.Contains("Topographies"))
            {
                foreach (TopographySurface ii in new FilteredElementCollector(doc).OfClass(typeof(TopographySurface)).Cast<TopographySurface>())
                {
                    if (ii.Pinned == !b)
                    {
                        ii.Pinned = b;
                        
                        nb_topo += 1;
                        
                    }

                }
                if (nb_topo > 1)
                    liste.Add(nb_topo.ToString() + " topographies");
                if (nb_topo == 1)
                    liste.Add("1 topographie");
            }


            if (liste_cat.Contains("Sols"))
            {
                foreach (Floor ii in new FilteredElementCollector(doc).OfClass(typeof(Floor)).Cast<Floor>())
                {
                    if (ii.Pinned == !b)
                    {
                        ii.Pinned = b;

                        nb_sols += 1;

                    }

                }
                if (nb_sols > 1)
                    liste.Add(nb_sols.ToString() + " sols");
                if (nb_sols == 1)
                    liste.Add("1 sol");
            }



            return liste;
        }

        
    }
}

