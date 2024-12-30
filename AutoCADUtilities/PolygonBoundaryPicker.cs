using Autodesk.AutoCAD.GraphicsInterface;

namespace DriveCadWithCode2025.AutoCADUtilities
{
    public class PolygonBoundaryPicker : PolygonPointsPicker
    {
        private readonly TransientManager _tsMng = 
            TransientManager.CurrentTransientManager;

        public PolygonBoundaryPicker(Editor ed, int boundaryColorIndex = 2) : 
            base(ed, boundaryColorIndex)
        { 

        }

        public override bool PickPolygon()
        {
            if(!base.PickPolygon()) return false;

            var ok = false;
            try
            {
                ThisEditor.PointMonitor += Editor_PointMonitor;
                while(true)
                {
                    Boundary = CreateGhostPolyline();
                    _tsMng.AddTransient(
                        Boundary, 
                        TransientDrawingMode.DirectTopmost, 
                        128, 
                        new IntegerCollection());

                    var opt = new PromptPointOptions("\nSelect polygon's next point:");
                    if (BoundaryPoints.Count>=3)
                    {
                        opt.AllowNone = true;
                        opt.Keywords.Add("Done");
                        opt.Keywords.Default = "Done";
                    }
                    var pRes = ThisEditor.GetPoint(opt);
                    if (pRes.Status== PromptStatus.OK)
                    {
                        BoundaryPoints.Add(pRes.Value);
                        ClearGhostPolyline();
                    }
                    else if (pRes.Status == PromptStatus.Keyword)
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
                ThisEditor.PointMonitor -= Editor_PointMonitor;
                ClearGhostPolyline();
            }

            return ok;
        }

        private void Editor_PointMonitor(object sender, PointMonitorEventArgs e)
        {
            if (Boundary == null || Boundary.NumberOfVertices < 3) return;

            var pt=new Point2d(e.Context.RawPoint.X, e.Context.RawPoint.Y);
            Boundary.SetPointAt(Boundary.NumberOfVertices-1, pt);
            _tsMng.UpdateTransient(Boundary, new IntegerCollection());
        }

        #region private methods

        private void ClearGhostPolyline()
        {
            if (Boundary != null)
            {
                _tsMng.EraseTransient(Boundary, new IntegerCollection());
                Boundary.Dispose();
                Boundary = null;
            }
        }

        #endregion
    }
}
