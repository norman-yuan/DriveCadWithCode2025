using Autodesk.AutoCAD.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DriveCadWithCode2025.AutoCADUtilities
{
    public static class AcadGenericUtilities
    {
        public static Transparency GetTransparencyByPercentage(int transparencyPercent)
        {
            if (transparencyPercent > 90) transparencyPercent = 90;
            var alpha = Convert.ToInt32(Math.Floor(255 * ((100 - transparencyPercent) / 100.0)));

            return new Transparency((byte)alpha);
        }
    }
}
