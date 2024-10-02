using Autodesk.AutoCAD.Runtime;
using OSGeo.MapGuide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveCadWithCode2025.MapUtilities
{
    public class CoordinateSystemConversion
    {
        public static void Convert(
            string sourceCoordCode, string destCoordCode, double xIn, double yIn, out double xOut, out double yOut)
        {
            xOut = 0.00;
            yOut = 0.00;

            MgCoordinateSystemFactory factory = null;
            MgCoordinateSystemCatalog catalog = null;
            MgCoordinateSystemDictionary coordDic = null;
            MgCoordinateSystem coordIn = null;
            MgCoordinateSystem coordOut = null;
            MgCoordinateSystemTransform coordTransform = null;
            MgCoordinate destCoord = null;

            try
            {

                factory = new MgCoordinateSystemFactory();

                catalog = factory.GetCatalog();
                coordDic = catalog.GetCoordinateSystemDictionary();

                coordIn = coordDic.GetCoordinateSystem(sourceCoordCode);
                coordOut = coordDic.GetCoordinateSystem(destCoordCode);

                coordTransform = factory.GetTransform(coordIn, coordOut);
                destCoord = coordTransform.Transform(xIn, yIn);

                yOut = destCoord.GetY();
                xOut = destCoord.GetX();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (factory != null) factory.Dispose();
                if (catalog != null) catalog.Dispose();
                if (coordDic != null) coordDic.Dispose();
                if (coordIn != null) coordIn.Dispose();
                if (coordOut != null) coordOut.Dispose();
                if (coordTransform != null) coordTransform.Dispose();
                if (destCoord != null) destCoord.Dispose();
            }
        }
    }
}
