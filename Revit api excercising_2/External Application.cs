using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Revit_api_excercising_2
{
    internal class External_Application : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {

            return Result.Succeeded;

        }

        public Result OnStartup(UIControlledApplication application)
        {

            application.CreateRibbonTab("Import Mechanical Systems From Linked CAD");

            RibbonPanel ribbonpanel = application.CreateRibbonPanel("Import Mechanical Systems From Linked CAD", "ITI-CEI");

            string path = Assembly.GetExecutingAssembly().Location;

            PushButtonData button = new PushButtonData("Button 1", "Draw Ducts", path, "Revit_api_excercising_2.Class1");

            PushButton pushButton = ribbonpanel.AddItem(button) as PushButton;

            Uri imgpath = new Uri(@"C:\_My Data\CEI Track\BIM development course\Revit api excercising_2\duct.png");
            //BitmapImage image = new BitmapImage(imgpath);

            //PushButton pushButton = ribbonpanel.AddItem(button) as PushButton;
            pushButton.LargeImage = new BitmapImage(imgpath);

            return Result.Succeeded;
        }
    }
}
