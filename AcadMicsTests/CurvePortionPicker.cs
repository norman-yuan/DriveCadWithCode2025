using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadMicsTests
{
    public class CurvePortionPicker : IDisposable
    {
        private readonly Document _dwg;
        private readonly Editor _ed;
        private TransientManager _tsm = TransientManager.CurrentTransientManager;

        //private Point3d _firstPoint;
        //private Point3d _secontPoint;

        private DBPoint _dbPoint1 = null;
        private DBPoint _dbPoint2 = null;
        
        private short _pdMode = 0;

        private Curve _curve;

        public CurvePortionPicker(Document dwg)
        {
            _dwg = dwg;
            _ed = _dwg.Editor;
            _pdMode = (short)CadApp.GetSystemVariable("PDMODE");
        }

        public void Dispose()
        {
            CadApp.SetSystemVariable("PDMODE", _pdMode);
            ClearTransients();
        }

        public bool PickCurvePortion(bool showpickedPoints=true)
        {
            if (showpickedPoints)
            {
                //CadApp.SetSystemVariable("PDMODE", 34);
            }

            if (!SelectFirstPoint(_ed, out Point3d firstPoint, out _curve))
            {
                return false;
            }

            _dbPoint1 = new DBPoint(firstPoint);
            _dbPoint1.ColorIndex = 2;
            _tsm.AddTransient(_dbPoint1, TransientDrawingMode.DirectTopmost, 128, new IntegerCollection());

            try
            {
                _ed.PointMonitor += Editor_PointMonitor;

                _dbPoint2=new DBPoint(firstPoint);
                _dbPoint2.ColorIndex = 1;
                _tsm.AddTransient(_dbPoint2, TransientDrawingMode.Highlight, 128, new IntegerCollection());

                var res = _ed.GetPoint("\nSelect another point on the curve:");
                if (res.Status== PromptStatus.OK)
                {
                    
                }
            }
            finally
            {
                _ed.PointMonitor -= Editor_PointMonitor;
            }

            return true;
        }

        private void Editor_PointMonitor(object sender, PointMonitorEventArgs e)
        {
            var currPoint = e.Context.RawPoint;
            if (currPoint.IsEqualTo(_dbPoint1.Position))
            {
                return;
            }

            var pt = _curve.GetClosestPointTo(currPoint, false);
            _dbPoint1.Position = pt;
            _tsm.UpdateTransient(_dbPoint2,new IntegerCollection());
        }

        #region private methods

        private bool SelectFirstPoint(Editor ed, out Point3d point, out Curve curve)
        {
            point = Point3d.Origin;
            curve = null;

            var opt = new PromptEntityOptions("\nSelect a curve:");
            opt.SetRejectMessage("\nInvalid selection: must be a curve entity.");
            opt.AddAllowedClass(typeof(Curve), false);
            var res = ed.GetEntity(opt);
            if (res.Status == PromptStatus.OK)
            {
                using var tran = new OpenCloseTransaction();
                curve = (Curve)tran.GetObject(res.ObjectId, OpenMode.ForRead).Clone();
                point = curve.GetClosestPointTo(res.PickedPoint, false);

                return true;
            }
            else
            {
                return false;
            }
        }

        private void ClearTransients()
        {
            if (_dbPoint1!=null)
            {

            }
        }

        #endregion
    }
}
