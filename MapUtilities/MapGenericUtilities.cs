using Autodesk.Gis.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapUtilities
{
    public class MapGenericUtilities
    {
        public static string GetDrawingCoordinateSystem(Document dwg)
        {
            var mapApp = HostMapApplicationServices.Application;
            var mapProj = mapApp.Projects.GetProject(dwg);
            return mapProj.Projection;
        }
    }
}
