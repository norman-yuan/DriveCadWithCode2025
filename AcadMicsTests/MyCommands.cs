
using Autodesk.AutoCAD.DatabaseServices.Filters;
using DriveCadWithCode2025.AutoCADUtilities;

[assembly: CommandClass(typeof(AcadMicsTests.MyCommands))]

namespace AcadMicsTests
{
    public class MyCommands 
    {
        private const string FILTER_DICT_NAME = "ACAD_FILTER";
        private const string SPATIAL_DICT_NAME = "SPATIAL";

        [CommandMethod("ClipBlk")]
        public static void ClipBlockReference()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var res = ed.GetEntity("\nSelect block reference to clip:");
            if (res.Status != PromptStatus.OK) return;

            if (RemoveClipFromBlockReference(res.ObjectId))
            {
                ed.Regen();
            }

            var opt = new PromptKeywordOptions("\nChoose clip boundary:");
            opt.AppendKeywordsToMessage = true;
            opt.Keywords.Add("Rectangle");
            opt.Keywords.Add("Polygon");
            opt.Keywords.Default = "Rectangle";
            var kres=ed.GetKeywords(opt);
            if (kres.Status != PromptStatus.OK) return;

            List<Point3d>? points = null;
            if (kres.StringResult == "Rectangle")
            {
                if (SelectClipWindow(ed, out Point3d pt1, out Point3d pt2))
                {
                    points=new List<Point3d> { pt1, pt2 };
                }
            }
            else
            {
                if (SelectClipPolygon(ed, out List<Point3d>? pts))
                {
                    if (pts != null)
                    {
                        points = pts;
                    }
                }
            }

            if (points == null) return;

            using (var tran = dwg.TransactionManager.StartTransaction())
            {
                var blk = (BlockReference)tran.GetObject(res.ObjectId, OpenMode.ForRead);
                if (blk.ExtensionDictionary.IsNull)
                {
                    blk.UpgradeOpen();
                    blk.CreateExtensionDictionary();
                }

                var extDict = (DBDictionary)tran.GetObject(
                    blk.ExtensionDictionary, OpenMode.ForWrite);
                DBDictionary filterDict;
                if (!extDict.Contains(FILTER_DICT_NAME))
                {
                    filterDict = new DBDictionary();
                    extDict.SetAt(FILTER_DICT_NAME, filterDict);
                    tran.AddNewlyCreatedDBObject(filterDict, true);
                }
                else
                {
                    filterDict=(DBDictionary)tran.GetObject(
                        extDict.GetAt(FILTER_DICT_NAME), OpenMode.ForWrite);
                }

                if (filterDict.Contains(SPATIAL_DICT_NAME))
                {
                    var id = filterDict.GetAt(SPATIAL_DICT_NAME);
                    filterDict.Remove(SPATIAL_DICT_NAME);
                    var spFilter = tran.GetObject(id, OpenMode.ForWrite);
                    spFilter.Erase();
                }

                Point2dCollection clipPoints = GetPolygonBoundary(points, blk.BlockTransform);

                var definition = new SpatialFilterDefinition(
                    clipPoints, Vector3d.ZAxis, 0.0,
                    double.PositiveInfinity, double.NegativeInfinity, true);
                var filter = new SpatialFilter();
                filter.Definition = definition;
                filterDict.SetAt(SPATIAL_DICT_NAME, filter);
                tran.AddNewlyCreatedDBObject(filter, true);

                tran.Commit();
            }

            ed.Regen();
            ed.UpdateScreen();
        }

        [CommandMethod("RemoveClip")]
        public static void RemoveClip()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var res = ed.GetEntity("\nSelect a clipped block reference:");
            if (res.Status != PromptStatus.OK) return;

            if (RemoveClipFromBlockReference(res.ObjectId))
            {
                ed.Regen();
            }
        }

        private static bool SelectClipWindow(Editor ed, out Point3d pt1, out Point3d pt2)
        {
            pt1 = Point3d.Origin;
            pt2 = Point3d.Origin;

            var res1 = ed.GetPoint("\nSelect lower-left corner:");
            if (res1.Status == PromptStatus.OK)
            {
                var res2 = ed.GetCorner("\nSelect upper-right corner:", res1.Value);
                if (res2.Status == PromptStatus.OK)
                {
                    pt1 = res1.Value;
                    pt2 = res2.Value;
                    return true;
                }
            }

            return false;
        }

        private static bool SelectClipPolygon(Editor ed, out List<Point3d>? points)
        {
            var picker = new PolygonAreaPicker(ed, 1, 25);
            if (picker.PickPolygon())
            {
                points = picker.BoundaryPoints;
                return true;
            }
            else
            {
                points = null;
                return false;
            }
        }

        private static bool RemoveClipFromBlockReference(ObjectId blkId)
        {
            var removed = false;
            var db = blkId.Database;
            using (var tran=db.TransactionManager.StartTransaction())
            {
                var blk = tran.GetObject(blkId, OpenMode.ForRead);
                if (!blk.ExtensionDictionary.IsNull)
                {
                    var extDict = (DBDictionary)tran.GetObject(
                        blk.ExtensionDictionary, OpenMode.ForRead);
                    if (extDict.Contains(FILTER_DICT_NAME))
                    {
                        var filterDict = (DBDictionary)tran.GetObject(
                            extDict.GetAt(FILTER_DICT_NAME), OpenMode.ForWrite);
                        if (filterDict.Contains(SPATIAL_DICT_NAME))
                        {
                            var filter = tran.GetObject(
                                filterDict.GetAt(SPATIAL_DICT_NAME), OpenMode.ForWrite);
                            filter.Erase();
                            filterDict.Remove(SPATIAL_DICT_NAME);
                        }

                        extDict.UpgradeOpen();
                        extDict.Remove(FILTER_DICT_NAME);

                        removed = true;
                    }
                }

                tran.Commit();
            }
            return removed;
        }

        private static Point2dCollection GetPolygonBoundary(
            IEnumerable<Point3d> points, Matrix3d blkTransform)
        {
            var pts = points
                .Select(pt => pt.TransformBy(blkTransform.Inverse()))
                .Select(pt => new Point2d(pt.X, pt.Y));
            Point2dCollection pt2ds = new Point2dCollection(pts.ToArray());
            return pt2ds;
        }

        #region following command crash AutoCAD: because SpatialFilter is "read-only"

        [CommandMethod("SetClip")]
        public static void RunMyCommand()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var res = ed.GetEntity("\nSelect a clipped block reference:");
            if (res.Status != PromptStatus.OK) return;

            if (!SelectClipWindow(ed, out Point3d pt1, out Point3d pt2)) return;

            var updated = false;
            using (var tran = dwg.TransactionManager.StartTransaction())
            {
                var blk = (BlockReference)tran.GetObject(res.ObjectId, OpenMode.ForRead);
                if (!blk.ExtensionDictionary.IsNull)
                {
                    var entDict = (DBDictionary)tran.GetObject(
                        blk.ExtensionDictionary, OpenMode.ForRead);
                    if (entDict.Contains("ACAD_FILTER"))
                    {
                        var sFilterDict = (DBDictionary)tran.GetObject(
                            entDict.GetAt("ACAD_FILTER"), OpenMode.ForRead);
                        var id = (ObjectId)sFilterDict["SPATIAL"];
                        var filter = (SpatialFilter)tran.GetObject(id, OpenMode.ForWrite);

                        var definition = CreateNewClipSpatialDefinition(
                            filter.Definition, pt1, pt2, blk.BlockTransform);
                        filter.Definition = definition;
                        updated = true;
                    }
                }

                tran.Commit();
            }

            if (updated)
            {
                ed.Regen();
            }
        }

        private static SpatialFilterDefinition CreateNewClipSpatialDefinition(
            SpatialFilterDefinition oldDef, Point3d pt1, Point3d pt2, Matrix3d blkTransform)
        {
            var pts = GetPolygonBoundary(
                new[]{ pt1, pt2}, blkTransform);
            var definition = new SpatialFilterDefinition(
                pts, oldDef.Normal, oldDef.Elevation, oldDef.FrontClip, oldDef.BackClip, oldDef.Enabled);
            return definition; ;
        }

        #endregion



        #region command to add custom ribbon tab

        [CommandMethod("MyRibbonTab")]
        public static void AddMyCustomRibbonTab()
        {
            try
            {
                var myRibbon = new MyRibbon();
                myRibbon.CreateMyTab();
            }
            catch (System.Exception ex)
            {
                CadApp.ShowAlertDialog($"Error:\n{ex.Message}");
            }
        }

        #endregion

        #region command to test PolygonPointsPicker

        [CommandMethod("PickPolygon")]
        public static void TestPolygonPicker()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var opt = new PromptKeywordOptions("Use Boundary Picker or Area Picker:");
            opt.AppendKeywordsToMessage = true;
            opt.Keywords.Add("Boundary");
            opt.Keywords.Add("Area");
            var res = ed.GetKeywords(opt);
            if (res.Status != PromptStatus.OK) return;

            try
            {
                PolygonPointsPicker picker;

                if (res.StringResult == "Boundary")
                {
                    picker = new PolygonBoundaryPicker(ed, 1);
                }
                else
                {
                    picker = new PolygonAreaPicker(ed, 1, 25);
                }

                if (picker.PickPolygon())
                {
                    var points = picker.BoundaryPoints;
                    ed.WriteMessage($"\nPicked points: {points.Count}");
                }
            }
            catch (System.Exception ex)
            {
                CadApp.ShowAlertDialog($"Error:\n{ex.Message}");
            }
        }

        #endregion
    }
}
