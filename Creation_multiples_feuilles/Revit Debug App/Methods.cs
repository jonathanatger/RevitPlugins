using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CreationMultiplesFeuilles.ViewModels;

namespace CreationMultiplesFeuilles
{

    /// <summary>
    /// Create methods here that need to be wrapped in a valid Revit Api context.
    /// Things like transactions modifying Revit Elements, etc.
    /// </summary>
    internal class Methods
    {

        public static void CreerFeuilles(Ui ui, Document doc)
        {
            
            //liste objet View Revit
            List<View> listevues = GetListViews(ui, doc);

            

            //liste cartouches
            ElementId idCartouche  = GetTitleBlock(ui, doc);            

            //Numéro correspondant au choix de préfix ou suffixe
            int valeur = GetNameAlterChoice(ui, doc);

            //prefixe et suffixe
            string prefixe = GetPrefixAndSuffix(ui, doc)[0];
            string suffixe = GetPrefixAndSuffix(ui, doc)[1];

            //Obtenir valeurs des paramètres de classement
            string classement1 = GetClassement(ui, doc)[0];
            string classement2 = GetClassement(ui, doc)[1];
            

            //Nom de la fenetre de vue sélectionnée
            string NomDeLaFenetreDeVue = GetViewPort(ui, doc);

            //préparation données
            List<string> sheetlist = new List<string>();
            List<string> sheetnumber = new List<string>();
            List<ElementId> selection_id_list = new List<ElementId>();
            int nombre_vues = 0;
            

            
            //Lancer la transaction Revit
            using (Transaction t = new Transaction(doc, "Créer feuilles"))
            {
                t.Start("Créer feuilles");

                

                {
                    foreach (View view in listevues)
                    {
                        nombre_vues += 1;
                      
                        //Obtenir les numéros de feuilles
                        foreach (ViewSheet vs in new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).Cast<ViewSheet>())
                        {
                            sheetnumber.Add(vs.SheetNumber.ToLower());
                        }

                        //Créer une feuille avec un numéro automatique
                        ViewSheet sheet;

                        if (idCartouche != null)
                        {
                            sheet = ViewSheet.Create(doc, idCartouche);
                        }
                        else
                        {
                            sheet = ViewSheet.Create(doc, ElementId.InvalidElementId);
                        }

                        //Renseigner paramètres de classement
                        try
                        {
                            sheet.LookupParameter(ui.valeurClassement1).Set(classement1);
                            sheet.LookupParameter(ui.valeurClassement2).Set(classement2);
                        }
                        catch { }
                        

                        //Choix du nom avec suffixe et prefixe
                        if (valeur == 0)
                        {
                            if (!view.Name.Contains("{") || !view.Name.Contains("}"))
                                sheet.Name = view.Name;
                            else sheet.Name = view.Name.Replace("{", "(").Replace("}", ")");

                            
                        }
                        if (valeur == 1)
                        {
                            if (!view.Name.Contains("{") || !view.Name.Contains("}"))
                                sheet.Name = prefixe + " " + view.Name;
                            else sheet.Name = prefixe + view.Name.Replace("{", "(").Replace("}", ")");
                        }
                        if (valeur == 2)
                        {
                            if (!view.Name.Contains("{") || !view.Name.Contains("}"))
                                sheet.Name = view.Name + " " + suffixe;
                            else sheet.Name = view.Name.Replace("{", "(").Replace("}", ")") + suffixe;
                        }

                        sheetlist.Add(sheet.SheetNumber + " - " + sheet.Name);

                        //Placement vue sur feuille, centré
                        BoundingBoxUV sheetBox = sheet.Outline;
                        double yPosition = (sheetBox.Max.V - sheetBox.Min.V) / 2 + sheetBox.Min.V;
                        double xPosition = (sheetBox.Max.U - sheetBox.Min.U) / 2 + sheetBox.Min.U;

                        //Placer les vues sur feuilles
                        if (view.CropBoxActive == false && view.CropBoxVisible == false)
                        {
                            view.CropBoxActive = true;
                            Viewport vp = Viewport.Create(doc, sheet.Id, view.Id, new XYZ(xPosition, yPosition, 0));
                            view.CropBoxActive = false;
                            view.CropBoxVisible = false;

                            ElementId vpid = getViewTypeId(doc, NomDeLaFenetreDeVue);
                            if (vpid != null)
                                vp.ChangeTypeId(vpid);

                        }
                        else if (view.CropBoxActive == false && view.CropBoxVisible == true)
                        {
                            view.CropBoxActive = true;
                            Viewport vp = Viewport.Create(doc, sheet.Id, view.Id, new XYZ(xPosition, yPosition, 0));
                            view.CropBoxActive = false;
                            view.CropBoxVisible = true;

                            ElementId vpid = getViewTypeId(doc, NomDeLaFenetreDeVue);
                            if (vpid != null)
                                vp.ChangeTypeId(vpid);
                        }
                        else
                        {
                            Viewport vp = Viewport.Create(doc, sheet.Id, view.Id, new XYZ(xPosition, yPosition, 0));

                            ElementId vpid = getViewTypeId(doc, NomDeLaFenetreDeVue);
                            if (vpid != null)
                                vp.ChangeTypeId(vpid);
                        }





                    }
                }

                //Message de validation
                TaskDialog td = new TaskDialog("Confirmation");
                td.AllowCancellation = true;
                td.TitleAutoPrefix = true;
                string nomdesvues = string.Join(Environment.NewLine, sheetlist);

                if (sheetlist.Count == 1)
                {
                    td.Title = "Confirmation";
                    td.MainInstruction = "Une nouvelle feuille va être créee :";
                    td.MainContent = sheetlist[0] + Environment.NewLine + "\nConfirmer la création de la feuille ?" ;
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

                if (sheetlist.Count > 1)
                {
                    td.Title = "Confirmation";
                    td.MainInstruction = sheetlist.Count + " nouvelles feuilles vont être créees : ";
                    td.MainContent = nomdesvues + Environment.NewLine + "\nConfirmer la création des feuilles ?";
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

        //obtiens nom cartouche
        public static ElementId GetTitleBlock(Ui ui, Document doc)
        {
            Vue vue = ui.lbxcartouche.SelectedItem as Vue;       

            return vue.Id;
        }

        
        public static string GetViewPort(Ui ui, Document doc)
        {
            Vue vue = ui.lbxfenetre.SelectedItem as Vue;

            return vue.Nom;
        }
        
        

        public static List<string> GetPrefixAndSuffix(Ui ui, Document doc)
        {
            List<string> list = new List<string>();
            string prefixe = ui.prefixeContent.Text;
            string suffixe = ui.suffixeContent.Text;

            list.Add(prefixe);
            list.Add(suffixe);

            return list;
        }

        public static int GetNameAlterChoice(Ui ui, Document doc)
        {
            int valeur = 0;
            
            if(ui.prefixe.IsChecked == true)
            {
                valeur = 1;
            }
            else if(ui.suffixe.IsChecked == true)
            {
                valeur = 2;
            }

            

            return valeur;
        }

        public static List<string> GetClassement(Ui ui, Document doc)
        {
           List<string> list = new List<string>();
            String l1 = "";
            String l2 = "";

            try          {
                
              
                list.Add(ui.lbxclassement1.SelectedItem.ToString());
            }
            catch
            {
                list.Add("");
            }

            try 
            {

                list.Add(ui.lbxclassement2.SelectedItem.ToString());
            }
            catch
            {
                list.Add("");
            }

            

            return list;
        }



    }


}

