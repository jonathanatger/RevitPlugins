using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SynchroniserPresentations.ViewModels;

namespace SynchroniserPresentations
{

    /// <summary>
    /// Create methods here that need to be wrapped in a valid Revit Api context.
    /// Things like transactions modifying Revit Elements, etc.
    /// </summary>
    internal class Methods
    {

        public static void SynchroniserLesPresentations(Ui ui, Document doc)
        {

            //préparation données
            ElementId rId = ui.referenceViewportId;
            ElementId sId = ui.selectedViewportId;
            Viewport rVp = doc.GetElement(rId) as Viewport;
            Viewport sVp = doc.GetElement(sId) as Viewport;
            Autodesk.Revit.DB.View rV = doc.GetElement(rVp.ViewId) as Autodesk.Revit.DB.View;
            Autodesk.Revit.DB.View sV = doc.GetElement(sVp.ViewId) as Autodesk.Revit.DB.View;

            bool rVCBox = rV.CropBoxActive;
            bool sVCBOX = sV.CropBoxActive;

            //Lancer la transaction Revit
            using (Transaction t = new Transaction(doc, "Synchroniser les présentations"))
            {
                t.Start("Synchroniser les présentations");

                //placer des cadrages pour obtenir plus de précision
                rV.CropBoxActive = true;
                sV.CropBoxActive = true;

                //obtenir les feuilles et les cartouches
                ViewSheet feuilleRef = doc.GetElement(rVp.SheetId) as ViewSheet;
                ViewSheet feuilleSel = doc.GetElement(sVp.SheetId) as ViewSheet;
                                
                Element selTitleBlock = new FilteredElementCollector(doc, sVp.SheetId).OfCategory(BuiltInCategory.OST_TitleBlocks).FirstElement();
                
                //placements de la feuille, du cartouche et de la vue de référence
                BoundingBoxUV refFeuilleBBox = feuilleRef.Outline;
                XYZ refVueXYZ = rVp.GetBoxCenter();

                double placementRefVueX = refVueXYZ.X - refFeuilleBBox.Min.U;
                double placementRefVueY = refVueXYZ.Y - refFeuilleBBox.Min.V;

                double XRefTitleBlock = ui.XRefTitleBlock;
                double YRefTitleBlock = ui.YRefTitleBlock;

                double distCoinCartoucheRefX = placementRefVueX - XRefTitleBlock;
                double distCoinCartoucheRefY = placementRefVueY - YRefTitleBlock;

                //placements de la feuille, du cartouche et de la vue sélectionnée
                BoundingBoxUV selFeuilleBBox = feuilleRef.Outline;
                XYZ selVueXYZ = sVp.GetBoxCenter();                

                double XSelTitleBlock = selTitleBlock.get_BoundingBox(feuilleSel).Min.X;
                double YSelTitleBlock = selTitleBlock.get_BoundingBox(feuilleSel).Max.Y;                
                
                double placementSelVueX = selVueXYZ.X - selFeuilleBBox.Min.U;
                double placementSelVueY = selVueXYZ.Y - selFeuilleBBox.Min.V;

                double distCoinCartoucheSelX = placementSelVueX - XSelTitleBlock;
                double distCoinCartoucheSelY = placementSelVueY - YSelTitleBlock;

                //coordo déplacement
                double deplacementX = distCoinCartoucheRefX - distCoinCartoucheSelX + selVueXYZ.X;
                double deplacementY = distCoinCartoucheRefY - distCoinCartoucheSelY + selVueXYZ.Y;
                XYZ déplXYZ = new XYZ(deplacementX, deplacementY, selVueXYZ.Z);
                         
                
                //déplacement de la vue
                sVp.SetBoxCenter(déplXYZ);                


                //rétablir les paramètres de cadrage de la vue 
                rV.CropBoxActive = rVCBox;
                sV.CropBoxActive = sVCBOX;


                t.Commit();
                t.Dispose();

            }
            

        }
        



    }


}

