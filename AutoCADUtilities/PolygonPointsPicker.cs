namespace DriveCadWithCode2025.AutoCADUtilities
{
    public abstract class PolygonPointsPicker
    {
        private readonly Editor _ed;
        private readonly List<Point3d> _points = new List<Point3d>();

        public PolygonPointsPicker(Editor ed, int boundaryColorIndex = 2)
        {
            _ed= ed;
            BoundaryColorIndex = boundaryColorIndex;
        }

        public List<Point3d> BoundaryPoints => _points;
        protected Editor ThisEditor => _ed;
        protected Polyline? Boundary { get; set; } = null;

        protected int BoundaryColorIndex { get; }

        public virtual bool PickPolygon()
        {
            if (!PickFirstTwoPoints())
            {
                _ed.WriteMessage("\n*Cancel*\n");
                return false;
            }
            else
            {
                return true;
            }
        }

        protected Polyline CreateGhostPolyline()
        {
            var poly = new Polyline();
            for (int i = 0; i < BoundaryPoints.Count; i++)
            {
                poly.AddVertexAt(i, new Point2d(
                    BoundaryPoints[i].X, BoundaryPoints[i].Y), 0.0, 0.0, 0.0);
            }

            poly.AddVertexAt(BoundaryPoints.Count, new Point2d(
                BoundaryPoints[BoundaryPoints.Count - 1].X, 
                BoundaryPoints[BoundaryPoints.Count - 1].Y), 0.0, 0.0, 0.0);

            poly.ColorIndex = BoundaryColorIndex;

            if (poly.NumberOfVertices > 2)
            {
                poly.Closed = true;
            }

            return poly;
        }

        private bool PickFirstTwoPoints()
        {
            var res = _ed.GetPoint("Select polygon's first point:");
            if (res.Status != PromptStatus.OK) return false;

            _points.Add(res.Value);

            var opt = new PromptPointOptions("\nSelect polygon's next point:");
            opt.UseBasePoint = true;
            opt.BasePoint = res.Value;
            opt.UseDashedLine = true;
            res = _ed.GetPoint(opt);
            if (res.Status != PromptStatus.OK) return false;

            _points.Add(res.Value);
            return true;
        }
    }
}
