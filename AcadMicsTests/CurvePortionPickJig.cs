using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadMicsTests
{
    public class CurvePortionPickJig : DrawJig, IDisposable
    {
        private readonly Document _dwg;
        private readonly Editor _ed;
        private Curve _curve = null;

        private Point3d _firstPoint;
        private Point3d _secontPoint;

        private Point3d _prevPoint;
        private Point3d _currPoint;

        public CurvePortionPickJig(Document dwg) : base()
        {
            _dwg = dwg;
            _ed = _dwg.Editor;
        }

        public void Dispose()
        {
            if (_curve!=null)
            {
                _curve.Dispose();
            }
        }

        public (Point3d firstPt, Point3d secondPt) PortionPoints
        {
            get
            {
                var d1=_curve.GetDistAtPoint(_firstPoint);
                var d2=_curve.GetDistAtPoint(_secontPoint);
                if (d2>d1)
                {
                    return (_firstPoint, _secontPoint);
                }
                else
                {
                    return (_secontPoint, _firstPoint);
                }
            }
        }
        
        public bool PickCurvePortion()
        {
            if (!SelectFirstPoint(_ed, out _firstPoint, out _curve))
            {
                return false;
            }

            _secontPoint = _firstPoint;
            _prevPoint = _firstPoint;
            _currPoint = _firstPoint;
            var jigResult = _ed.Drag(this);
            if (jigResult.Status == PromptStatus.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var opt = new JigPromptPointOptions(
                "\nSelect another point on the curve:");
            var res = prompts.AcquirePoint(opt);
            if (res.Status == PromptStatus.OK)
            {
                var ptOnCurve = _curve.GetClosestPointTo(res.Value, false);
                if (ptOnCurve.IsEqualTo(_firstPoint))
                {
                    return SamplerStatus.NoChange;
                }
                else
                {
                    _currPoint = ptOnCurve;
                    return SamplerStatus.OK;
                }
            }
            else
            {
                return SamplerStatus.Cancel;
            }
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            var splitCurves = SplitCurve();
            if (splitCurves.Count == 1)
            {
                draw.SubEntityTraits.Color = 2;
                draw.Geometry.Curve(splitCurves[0].GetGeCurve());
            }
            else if (splitCurves.Count == 2)
            {
                draw.SubEntityTraits.Color = 2;
                draw.Geometry.Curve(splitCurves[0].GetGeCurve());

                draw.SubEntityTraits.Color = 1;
                draw.Geometry.Curve(splitCurves[1].GetGeCurve());
            }
            else if (splitCurves.Count == 3)
            {
                draw.SubEntityTraits.Color = 1;
                draw.Geometry.Curve(splitCurves[0].GetGeCurve());

                draw.SubEntityTraits.Color = 2;
                draw.Geometry.Curve(splitCurves[1].GetGeCurve());

                draw.SubEntityTraits.Color = 1;
                draw.Geometry.Curve(splitCurves[2].GetGeCurve());
            }

            return true;
        }



        #region private methods

        private bool SelectFirstPoint(Editor ed, out Point3d point, out Curve curve)
        {
            point = Point3d.Origin;
            curve = null;

            var opt = new PromptEntityOptions("\nSelect a curve:");
            opt.SetRejectMessage("\nInvalid selection: must be a curve entity.");
            opt.AddAllowedClass(typeof(Curve), false);
            var res=ed.GetEntity(opt);
            if (res.Status== PromptStatus.OK)
            {
                using var tran = new OpenCloseTransaction();
                curve = tran.GetObject(res.ObjectId, OpenMode.ForRead).Clone() as Curve;
                point = curve.GetClosestPointTo(res.PickedPoint, false);
                
                return true;
            }
            else
            {
                return false;
            }
        }

        private List<Curve> SplitCurve()
        {
            var curves=new List<Curve>();
            var d1 = _curve.GetDistAtPoint(_firstPoint);
            var d2 = _curve.GetDistAtPoint(_currPoint);
            Point3dCollection points;
            if (d2>d1)
            {
                points=new Point3dCollection(new[] {_firstPoint, _currPoint});
            }
            else
            {
                points=new Point3dCollection(new[] {_currPoint, _firstPoint});
            }

            var splits = _curve.GetSplitCurves(points);
            foreach (DBObject obj in splits)
            {
                curves.Add(obj as Curve);
            }
            if (d1>d2)
            {
                curves.Reverse();
            }
            return curves;
        }

        #endregion
    }
}
