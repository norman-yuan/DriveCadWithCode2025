
using Autodesk.AutoCAD.DatabaseServices.Filters;
using DriveCadWithCode2025.AutoCADUtilities;
using RestSharp;

[assembly: CommandClass(typeof(AcadMicsTests.MyCommands))]

namespace AcadMicsTests
{
    public class MyCommands 
    {
        private const string FILTER_DICT_NAME = "ACAD_FILTER";
        private const string SPATIAL_DICT_NAME = "SPATIAL";

        #region Clipping block reference

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
                filter.Inverted = true;

                filterDict.SetAt(SPATIAL_DICT_NAME, filter);
                tran.AddNewlyCreatedDBObject(filter, true);

                tran.Commit();
            }

            ed.Regen();
            ed.UpdateScreen();

            GetExistingSpatialFilter(res.ObjectId);
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

        private static void GetExistingSpatialFilter(ObjectId entId)
        {
            using (var tran = entId.Database.TransactionManager.StartTransaction())
            {
                var ent = (Entity)tran.GetObject(entId, OpenMode.ForRead);
                if (!ent.ExtensionDictionary.IsNull)
                {
                    var extDict = (DBDictionary)tran.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);
                    if (extDict.Contains(FILTER_DICT_NAME))
                    {
                        var filterDict = (DBDictionary)tran.GetObject(extDict.GetAt(FILTER_DICT_NAME), OpenMode.ForRead);
                        if (filterDict.Contains(SPATIAL_DICT_NAME))
                        {
                            var spFilter = (SpatialFilter)tran.GetObject(filterDict.GetAt(SPATIAL_DICT_NAME), OpenMode.ForRead);
                            var inverted = spFilter.Inverted;
                        }
                    }
                }
                tran.Commit();
            }
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

        #region Determine COPY/PASTE source file

        private static IntPtr _copySourceDoc = IntPtr.Zero;
        private static bool _isCopyClip = false;

        [CommandMethod("HandlePaste")]
        public static void GetDataInClipboard()
        {
            foreach (Document dwg in CadApp.DocumentManager)
            {
                var db = dwg.Database;

                db.WblockNotice += Db_WblockNotice;
                dwg.CommandWillStart += Dwg_CommandWillStart;
                dwg.CommandEnded += Dwg_CommandEnded;
            }

            CadApp.DocumentManager.DocumentCreated += (o, e) =>
            {
                var dwg = e.Document;
                var db = dwg.Database;

                db.WblockNotice += Db_WblockNotice;
                dwg.CommandWillStart += Dwg_CommandWillStart;
                dwg.CommandEnded += Dwg_CommandEnded;
            };
        }

        private static void Db_WblockNotice(object sender, WblockNoticeEventArgs e)
        {
            if (_isCopyClip)
            {
                _copySourceDoc = CadApp.DocumentManager.MdiActiveDocument.UnmanagedObject;
            }
        }

        private static void Dwg_CommandEnded(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName.ToUpper().Contains("COPYCLIP") ||
                e.GlobalCommandName.ToUpper().Contains("COPYBASE"))
            {
                _isCopyClip = false;
            }
        }

        private static void Dwg_CommandWillStart(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName.ToUpper().Contains("COPYCLIP") ||
                e.GlobalCommandName.ToUpper().Contains("COPYBASE"))
            {
                _isCopyClip = true;
            }
            else if (e.GlobalCommandName.ToUpper().Contains("PASTECLIP") ||
                e.GlobalCommandName.ToUpper().Contains("PASTEORIG"))
            {
                var sourceFile = "";
                foreach (Document dwg in CadApp.DocumentManager)
                {
                    if (dwg.UnmanagedObject == _copySourceDoc)
                    {
                        sourceFile = dwg.Name;
                        break;
                    }
                }

                CadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage(
                    $"\nYou are to paste objects copied from:\n{sourceFile}...\n");
            }
        }

        #endregion

        #region RestSharp client test

        [CommandMethod("RestSharpTest")]
        public static async void TestRestSharp()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var baseUrl = "https://www.bing.com";
            var options = new RestClientOptions(baseUrl)
            {
                ThrowOnAnyError = false
            };

            using (var restSharp = new RestSharp.RestClient(options))
            {
                var data = await restSharp.GetAsync(new RestRequest(""));
            }
        }

        [CommandMethod("TestEntitlement")]
        public static async void TestAppEntitlement()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var userId = CadApp.GetSystemVariable("ONLINEUSERID").ToString();
            var appId = "250987619170000000";
            var isOk = await IsAppEntitled(userId, appId);
        }

        private static async Task<bool> IsAppEntitled(string userId, string appId)
        {
            var ok = false;
            //var baseUrl = "https://apps.autodesk.com/webservices/checkentitlement";
            var baseUrl = "https://apps.autodesk.com";
            var options = new RestClientOptions(baseUrl)
            {
                ThrowOnAnyError = false
            };
            var request = new RestRequest("webservices/checkentitlement");
            request.AddQueryParameter("userId", userId);
            request.AddQueryParameter("appId", appId);

            using (var restSharp = new RestSharp.RestClient(options))
            {
                try
                {
                    var response = await restSharp.GetAsync(request);
                    // ... decide if the response means entitled or not
                    ok = true;
                }
                catch (System.Exception ex)
                {
                    CadApp.ShowAlertDialog(ex.Message);
                    ok = false;
                }
            }
            return ok;
        }

        #endregion

        #region pick portion of a curve

        [CommandMethod("PickCurvePortion")]
        public static void SelectPortionOfCurve()
        {
            var dwg=CadApp.DocumentManager.MdiActiveDocument;

            using (var picker = new CurvePortionPicker(dwg))
            {
                if (picker.PickCurvePortion())
                {

                }
            }
        }

        #endregion
    }
}
