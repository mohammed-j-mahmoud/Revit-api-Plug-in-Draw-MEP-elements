using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Mechanical;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Exceptions;


namespace Revit_api_excercising_2
{
    [TransactionAttribute(TransactionMode.Manual)]
    
    public class Class1: IExternalCommand
    {
        [Obsolete]
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            double tolerance = app.ShortCurveTolerance;
            TaskDialog.Show("Tips", "please Pick Linked CAD File");
            // Pick Import Instance
            ImportInstance import = null;
            try
            {
                Reference r = uidoc.Selection.PickObject(ObjectType.Element, new Util.ElementsOfClassSelectionFilter<ImportInstance>());
                import = doc.GetElement(r) as ImportInstance;
            }
            catch
            {
                return Result.Cancelled;
            }
            if (import == null)
            {
                System.Windows.MessageBox.Show("CAD not found", "Tips");
                return Result.Cancelled;
            }
            

            // Fetch baselines
            List<Curve> wallCrvs = new List<Curve>();
            var wallLayers = Util.Misc.GetLayerNames(Properties.Settings.Default.layerWall);
            try
            {
                foreach (string wallLayer in wallLayers)
                {
                    wallCrvs.AddRange(Util.TeighaGeometry.ShatterCADGeometry(uidoc, import, wallLayer, tolerance));
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "Tips");
                return Result.Cancelled;
            }
            if (wallCrvs == null || wallCrvs.Count == 0)
            {
                System.Windows.MessageBox.Show("Baselines not found", "Tips");
                return Result.Cancelled;
            }


            // Grab the current building level
            FilteredElementCollector docLevels = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.INVALID)
                .OfClass(typeof(Level));
            ICollection<Element> levels = docLevels.OfClass(typeof(Level)).ToElements();
            Level defaultLevel = null;
            foreach (Level level in levels)
            {
                if (level.Id == import.LevelId)
                {
                    defaultLevel = level;
                }
            }
            if (defaultLevel == null)
            {
                System.Windows.MessageBox.Show("Please make sure there's a base level in current view", "Tips");
                return Result.Cancelled;
            }


            MEPSystemType mepSystemType = new FilteredElementCollector(doc)
            .OfClass(typeof(MEPSystemType))
            .Cast<MEPSystemType>()
            .FirstOrDefault(sysType => sysType.SystemClassification == MEPSystemClassification.SupplyAir);


            TaskDialog dialog2 = new TaskDialog("Duct Type");
            dialog2.MainContent = "Do you Want to Selecte Duct type?";
            dialog2.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            if (dialog2.Show() == TaskDialogResult.Yes)
            {

                TaskDialog.Show("Tips", "please Pick Duct");
                // Pick Duct Type
                MEPCurve importDuct = null;
                try
                {
                    Reference rDuct = uidoc.Selection.PickObject(ObjectType.Element, new Util.ElementsOfClassSelectionFilter<MEPCurve>());
                    importDuct = doc.GetElement(rDuct) as MEPCurve;
                    
                   
                }
                catch
                {
                    return Result.Cancelled;
                }
                if (import == null)
                {
                    System.Windows.MessageBox.Show("DUCT not found", "Tips");
                    return Result.Cancelled;
                    
                }
                Transaction tg = new Transaction(doc, "Draw MEP");
                tg.Start();
                ElementId eleid = importDuct.GetTypeId();
                foreach (Curve e in wallCrvs)
                {
                    XYZ LineStartPoint = e.GetEndPoint(0);
                    XYZ LineEndPoint = e.GetEndPoint(1);
                    Duct duct = Duct.Create(doc, mepSystemType.Id, eleid, defaultLevel.Id, LineStartPoint, LineEndPoint);
                }
                tg.Commit();

                
                TaskDialog dialog = new TaskDialog("Delete Elements");
                dialog.MainContent = "Do you Want to Delete The Selected Duct?";
                dialog.CommonButtons = TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel;
                if (dialog.Show() == TaskDialogResult.Ok)
                {
                    Transaction trans = new Transaction(doc, "Delete Selected Duct");
                    trans.Start();
                    doc.Delete(importDuct.Id);
                    TaskDialog.Show("Tips", "Duct Deleted");
                    trans.Commit();

                }

            }
            else 
            {
               
                 Transaction trans2 = new Transaction(doc, "Draw MEP");
                 FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(DuctType)).WhereElementIsElementType();
                 DuctType DT = collector.First() as DuctType;
                 trans2.Start();

                 foreach (Curve e in wallCrvs)
                 {
                     XYZ LineStartPoint = e.GetEndPoint(0);
                     XYZ LineEndPoint = e.GetEndPoint(1);
                     Duct duct = Duct.Create(doc, mepSystemType.Id, DT.Id, defaultLevel.Id, LineStartPoint, LineEndPoint);
                 }
                 TaskDialog.Show("Tips", "Done, Defult Duct Type Used");
                 trans2.Commit();
                
            }
           

            return Result.Succeeded;
        }

        
    }
}
