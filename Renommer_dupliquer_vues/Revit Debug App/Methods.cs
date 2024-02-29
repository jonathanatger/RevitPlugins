using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RenommerDupliquerVues.ViewModels;

namespace RenommerDupliquerVues
{

    /// <summary>
    /// Create methods here that need to be wrapped in a valid Revit Api context.
    /// Things like transactions modifying Revit Elements, etc.
    /// </summary>
    internal class Methods
    {

        public static void RenommerDupliquer(Ui ui, Document doc)
        {

            //liste objet View Revit
            
            List<View> listevues = new List<View>();
            List<View> listevuesdupliquees = new List<View>();
            List<string> listevuescreees = new List<string>();
            List<string> listevuesnoncreees = new List<string>();
            List<List<string>> listes = new List<List<string>>();
            



            //préparation données
            List<string> sheetlist = new List<string>();            
            List<ElementId> selection_id_list = new List<ElementId>();
            int choix = 0;
            
            
            

            
            //Lancer la transaction Revit
            using (Transaction t = new Transaction(doc, "Renommer/Dupliquer des feuilles"))
            {
                t.Start("Renommer/Dupliquer des feuilles");

                {
                    //Obtenir liste vues
                    listevues = GetListViews(ui, doc);

                    // Choix de la méthode de renommage
                    if (ui.insertion.IsChecked == true)
                    {
                        choix = 0;
                    }
                    else if (ui.remplacement.IsChecked == true)
                    {
                        choix = 1;
                    }
                    else if (ui.suppression.IsChecked == true)
                    {
                        choix = 2;
                    }

                    listes = RenommerVues(ui, doc, listevues, choix);
                    listevuescreees = listes[0];
                    listevuesnoncreees = listes[1];

                    

                    if (listevuesnoncreees.Count() == 0)
                    {
                        //lancer les méthodes
                        if (ui.renommer.IsChecked == true)
                        {                            
                            
                            AppliquerNomsVues(ui, listevues, listevuescreees);
                            AppliquerGabarit(ui, doc, listevues, listevues);
                        }
                        else if (ui.dupliquer.IsChecked == true)
                        {
                            listevuesdupliquees = DupliquerVues(ui, doc, listevues, listevuescreees, 0);
                            
                            AppliquerGabarit(ui, doc, listevuesdupliquees, listevues);

                        }
                        else if (ui.dupliquerdetails.IsChecked == true)
                        {
                            listevuesdupliquees = DupliquerVues(ui, doc, listevues, listevuescreees, 1);
                            
                            AppliquerGabarit(ui, doc, listevuesdupliquees, listevues);
                        }
                        else
                        {
                            listevuesdupliquees = DupliquerVues(ui, doc, listevues, listevuescreees, 2);
                            
                            AppliquerGabarit(ui, doc, listevuesdupliquees, listevues);
                        }
                    }                                      
                }

                //Message de validation
                TaskDialog td = new TaskDialog("Confirmation");
                td.AllowCancellation = true;
                td.TitleAutoPrefix = true;
                string nomdesvues = ""; string verbe;
                if (ui.renommer.IsChecked == true) { verbe = "renommée"; }
                else { verbe = "créée"; }


                //WIP ------------------- ATTENTION A T.COMMIT()
                if (listevuesnoncreees.Count == 0)
                {
                    nomdesvues = string.Join(Environment.NewLine, listevuescreees);                    

                    td.Title = "Confirmation";
                    if(listevuescreees.Count == 1)
                    {
                        td.MainInstruction = "Voici le nom de la vue qui va être " + verbe + " :";
                        td.MainContent = listevuescreees[0] + Environment.NewLine + "\nConfirmer le nom ?";

                    } else
                    {
                        td.MainInstruction = "Voici les noms des " + listevuescreees.Count.ToString() + " vues qui vont être "  + verbe + "s :";
                        td.MainContent = nomdesvues + Environment.NewLine + "\nConfirmer les noms ?";
                    }                    
                    
                    td.CommonButtons = TaskDialogCommonButtons.Cancel | TaskDialogCommonButtons.Ok;
                    TaskDialogResult result = td.Show();
                    if (result == TaskDialogResult.Ok)
                    {   
                        t.Commit();
                        ui.Close();
                    } else if(result == TaskDialogResult.Cancel)
                    {
                        ui.Topmost = true;  // important
                        ui.Topmost = false; // important
                        ui.Focus();
                    }                        
                }

                else 
                {
                    nomdesvues = string.Join(Environment.NewLine, listevuesnoncreees);

                    td.Title = "Attention";
                    if (listevuesnoncreees.Count == 1)
                    {
                        td.MainInstruction = "Parmi les vues demandées, une vue ne peut pas être " + verbe + ", parce que le nom existe déjà. Vous trouverez le nom ci-dessous :";
                    } else
                    {
                        td.MainInstruction = "Parmi les vues demandées, " + listevuesnoncreees.Count.ToString() + " vues ne peuvent pas être " + verbe + "s, parce que leur nom existe déjà. Vous trouverez les noms ci-dessous :";

                    }
                        
                    td.MainContent = nomdesvues + Environment.NewLine;
                    td.CommonButtons = TaskDialogCommonButtons.Cancel;
                    TaskDialogResult result = td.Show();
                    if (result == TaskDialogResult.Ok)
                    {
                        t.Commit();
                        ui.Close();

                    } else if(result == TaskDialogResult.Cancel)
                    {
                        ui.Topmost = true;  // important
                        ui.Topmost = false; // important
                        ui.Focus();

                    }      
                }               

                //Terminer la transaction               
                t.Dispose();
            }
            

        }


        //Obtenir l'Id de certains éléments
        public static ElementId getViewTypeId(Document doc, string nom)
        {
            ElementId viewPortType = null;
            foreach (Element elem in new FilteredElementCollector(doc).WhereElementIsElementType())
            {
                if (elem.Name == nom)
                {
                    viewPortType = elem.Id;
                    break;
                }
            }
            return viewPortType;
        }

        //Obtenir les vues, cartouches et types de fenetre séléctionnés dans l'UI
        public static List<View> GetListViews(Ui ui, Document doc)
        {
            
            List<View> vliste_3 = new List<View>();

            List<Vue> liste = new List<Vue>();
            foreach(object v in ui.lbxvues.SelectedItems)
            {
                liste.Add(v as Vue);
            }
            

            foreach (Vue v in liste)
            {
                vliste_3.Add(doc.GetElement(v.Id) as View);                
            }
            
            return vliste_3;
        }

        //Crée les nouveaux noms de vues et vérifie qu'ils ne sont pas déja utilisés
        public static List<List<string>> RenommerVues(Ui ui, Document doc, List<View> listevues, int choix)

        {
            string nouveaulabel = "";
            List<string> viewlist_global = new List<string>();
            List<string> vuescreees = new List<string>();
            List<string> vuesnoncreees = new List<string>();
            List<List<string>> retour = new List<List<string>>();

            // liste de tout les noms de vues existantes
            foreach (View v in new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>())
            {
                if (v.ViewType != ViewType.Schedule && !(v.IsTemplate))
                    viewlist_global.Add(v.ViewType + v.Name);
            }

            //incrément
            int i = 0;

            //Boucle pour changer le nom de chaque vue
            foreach (View view in listevues)
            {
                nouveaulabel = ui.ModificationNom(i)[1];                              
                                
                //Check si le nom de la vue existe déja
                if (!viewlist_global.Contains(view.ViewType + nouveaulabel))
                {
                    vuescreees.Add(nouveaulabel);                    
                }
                else 
                {
                    vuesnoncreees.Add(nouveaulabel);
                }
                //incrément
                i++;
            }

            retour.Add(vuescreees);
            retour.Add(vuesnoncreees);
            //renvoye la liste de vues non créées
            return retour;
        }

        //Appliquer une liste de noms à des vues
        public static void AppliquerNomsVues (Ui ui, List<View> listevues, List<string> listenoms)
        {
            int i = 0;            
            
            foreach (View v in listevues)
            {
                v.Name = listenoms[i];               

                i = i + 1;

            }  
        }

        //Prends une liste de vues, la duplique et lui donne les noms correspondants. Le paramètre 'choix' détermine comment elles sont dupliquées
        public static List<View> DupliquerVues (Ui ui, Document doc, List<View> listevues, List<string> listenoms, int choix)
        {
            List<View> listevuesdupliquees = new List<View>();

            int i = 0;

            foreach(View view in listevues)
            {
                //Dupliquer vues
                if (choix == 0)
                {
                    View v2 = doc.GetElement(view.Duplicate(ViewDuplicateOption.Duplicate)) as View;
                    v2.Name = listenoms[i];
                    listevuesdupliquees.Add(v2);
                }
                //Dupliquer vues avec détails
                else if (choix == 1)
                {
                    View v2 = doc.GetElement(view.Duplicate(ViewDuplicateOption.WithDetailing)) as View;
                    v2.Name = listenoms[i];
                    listevuesdupliquees.Add(v2);
                }
                //Dupliquer en tant que vue dépendante
                else if (choix == 2)
                {
                    View v2 = doc.GetElement(view.Duplicate(ViewDuplicateOption.AsDependent)) as View;
                    v2.Name = listenoms[i];
                    listevuesdupliquees.Add(v2);
                }

                i++;
            }           

            return listevuesdupliquees;
        }

        //Appliquer le gabarit choisi dans l'UI aux vues 
        public static void AppliquerGabarit (Ui ui, Document doc, List<View> listevues, List<View> listevuesoriginales)
        {
            ElementId idgabarit = null;
            int i = 0;

            try 
            {
                Vue vue = ui.cbxgabarit.SelectedItem as Vue;
                idgabarit = vue.Id;
            }
            catch { }

            foreach (View v in listevues)
            {
                if(!(idgabarit == null))
                {               
                    try { v.ViewTemplateId = idgabarit; }
                    catch { }
                }

                i++;
            }
        }
    }
}

