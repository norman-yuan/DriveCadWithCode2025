using Autodesk.AutoCAD.GraphicsInterface;
using CadDb = Autodesk.AutoCAD.DatabaseServices;

namespace DriveCadWithCode2025.AutoCADUtilities
{
    public class PolygonAreaPicker : PolygonPointsPicker
    {
        private class PolygonPointJig : DrawJig
        {
            private CadDb.Polyline _polyline;
            private Point3d _prevPoint;
            private Point3d _currPoint;
            private readonly int _transparencyPercentile;
            public PolygonPointJig(CadDb.Polyline polyline, int backgroundTransparency = 50)
            {
                _polyline = polyline;
                _transparencyPercentile = backgroundTransparency;
                var lastPt = _polyline.GetPoint3dAt(_polyline.NumberOfVertices - 1);
                _prevPoint = lastPt;
                _currPoint = lastPt;
            }

            public Point3d CurrentPoint => _currPoint;

            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                var opt = new JigPromptPointOptions("\nSelect polygon's next point:");
                if (_polyline.NumberOfVertices > 3)
                {
                    opt.UserInputControls = UserInputControls.NullResponseAccepted;
                    opt.Keywords.Add("Done");
                    opt.Keywords.Default = "Done";
                }

                var res = prompts.AcquirePoint(opt);
                if (res.Status == PromptStatus.OK)
                {
                    if (res.Value.IsEqualTo(_prevPoint))
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        _prevPoint = _currPoint;
                        _currPoint = res.Value;
                        return SamplerStatus.OK;
                    }
                }
                else if (res.Status == PromptStatus.Keyword)
                {
                    return SamplerStatus.OK;
                }
                else
                {
                    return SamplerStatus.Cancel;
                }
            }

            protected override bool WorldDraw(WorldDraw draw)
            {
                draw.Geometry.Draw(_polyline);

                var pt = new Point2d(_currPoint.X, _currPoint.Y);
                _polyline.SetPointAt(_polyline.NumberOfVertices - 1, pt);

                Point3dCollection pts = new Point3dCollection();
                for (int i = 0; i < _polyline.NumberOfVertices; i++)
                {
                    pts.Add(_polyline.GetPoint3dAt(i));
                }

                draw.SubEntityTraits.FillType = FillType.FillAlways;
                draw.SubEntityTraits.Transparency =
                    AcadGenericUtilities.GetTransparencyByPercentage(_transparencyPercentile);
                draw.SubEntityTraits.Color = 2;
                draw.Geometry.Polygon(pts);

                return true;
            }
        }

        private int _transparencyPercentile = 50;

        public PolygonAreaPicker(
            Editor ed, 
            int boundaryColorIndex = 2,  
            int backgroundTransparency=50) : base(ed, boundaryColorIndex)
        {
            _transparencyPercentile = backgroundTransparency;
        }

        public override bool PickPolygon()
        {
            if (!base.PickPolygon()) return false;

            var ok = false;

            try
            {
                while(true)
                {
                    Boundary = CreateGhostPolyline();
                    var jig = new PolygonPointJig(Boundary);
                    var jigRes = ThisEditor.Drag(jig);
                    if (jigRes.Status== PromptStatus.OK)
                    {
                        BoundaryPoints.Add(jig.CurrentPoint);
                        Boundary.Dispose();
                        Boundary = null;
                        jig = null;
                    }
                    else if (jigRes.Status == PromptStatus.Keyword)
                    {
                        ok = true;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            finally
            {
                if (Boundary != null)
                {
                    Boundary.Dispose();
                }
            }

            return ok;
        }
    }
}
