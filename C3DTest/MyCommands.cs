
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

[assembly: ExtensionApplication(typeof(C3DTest.MyCommands))]
[assembly: CommandClass(typeof(C3DTest.MyCommands))]

namespace C3DTest
{
    public class MyCommands : IExtensionApplication
    {
        public void Initialize()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            if (dwg != null)
            {
                var ed = dwg.Editor;
                ed.WriteMessage("\nInitializing C3DTest.dll...");
                try
                {
                    

                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\nInitializing error:\n{ex.Message}");
                }
            }

            CadApp.DocumentManager.DocumentCreated += DocumentManager_DocumentCreated;
            foreach (Document d in CadApp.DocumentManager)
            {
                d.Database.ObjectModified += Database_ObjectModified;
            }
        }

        private void DocumentManager_DocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            e.Document.Database.ObjectOpenedForModify += Database_ObjectOpenedForModify;
            e.Document.Database.ObjectModified += Database_ObjectModified;
        }

        private void Database_ObjectOpenedForModify(object sender, ObjectEventArgs e)
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg == null ? null : dwg.Editor;

            if (e.DBObject is Structure str)
            {
                var handle = $"@@@@@@@@ opening STRUCTURE for modification: {str.Handle}";
                if (ed != null)
                {
                    ed.WriteMessage($"\n{handle}");
                }
            }
        }

        private void Database_ObjectModified(object sender, ObjectEventArgs e)
        {
            var dwg=CadApp.DocumentManager.MdiActiveDocument;
            var ed=dwg==null? null : dwg.Editor;  
            
            if (e.DBObject is Structure st)
            {
                var handle = $"++++++++++ Modified STRUCTURE: {st.Handle}";
                if (ed!=null)
                {
                    ed.WriteMessage($"\n{handle}");
                }
            }
            else if (e.DBObject is Pipe pipe)
            {
                var handle = $"--------- Modified PIPE: {pipe.Handle}";
                if (ed != null)
                {
                    ed.WriteMessage($"\n{handle}");
                }
            }
            //else
            //{
            //    if (ed != null)
            //    {
            //        ed.WriteMessage($"\nEntity modified: {e.DBObject.ObjectId.ObjectClass.DxfName}");
            //    }
            //}
        }

        public void Terminate()
        {

        }

        [CommandMethod("MyCmd")]
        public static void RunMyCommand()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            ed.WriteMessage("\nHello from AutoCAD 2025");
        }

        private NetworkChangeTracker _tracker = null;
        private bool _idleHooked = false;
        [CommandMethod("TrackNetwork")]
        public void TrackNetworkChange()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var cdoc = CivilApplication.ActiveDocument;
            var netIds=cdoc.GetPipeNetworkIds();
            var netNames=new List<string>();
            using (var tran = dwg.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in netIds)
                {
                    var net = (Network)tran.GetObject(id, OpenMode.ForRead);
                    netNames.Add(net.Name);
                }
                tran.Commit();
            }

            if (_tracker == null)
            {
                _tracker = new NetworkChangeTracker(dwg);
                _tracker.NetworkChanged += _tracker_NetworkChanged;
                _idleHooked = false;

                foreach (var name in netNames)
                {
                    _tracker.AddTrackingTarget(name);
                }
            }

            if (!_tracker.IsEnabled)
            {
                _tracker.EnableTracling();
                dwg.Editor.WriteMessage("\nNetworkChangeTracker is enabled!");
            }
            else
            {
                _tracker.DisableTracking();
                dwg.Editor.WriteMessage("\nNetworkChangeTracker is disabled!");
            }
        }

        private void _tracker_NetworkChanged(object sender, NetworkChangeEventArgs e)
        {
            if (!_idleHooked)
            {
                CadApp.Idle += CadApp_Idle;
                _idleHooked = true;
            }
        }

        private void CadApp_Idle(object? sender, EventArgs e)
        {
            CadApp.Idle -= CadApp_Idle;

            try
            {
                _tracker.DisableTracking();

                var dwg = CadApp.DocumentManager.MdiActiveDocument;
                if (dwg == null || !dwg.IsActive)
                {
                    return;
                }

                dwg.Editor.WriteMessage("\n===========================================");
                dwg.Editor.WriteMessage("\nNetwork chnage detected.");
                dwg.Editor.WriteMessage($"\nAdded parts: {_tracker.AddedParts.Count}");
                dwg.Editor.WriteMessage($"\nModified parts: {_tracker.ModifiedParts.Count}");
                dwg.Editor.WriteMessage($"\nErased parts: {_tracker.ErasedParts.Count}");
                dwg.Editor.WriteMessage("\n===========================================");
            }
            finally
            {
                _idleHooked = false;
                _tracker.ResetTrackedParts();
                _tracker.EnableTracling();
            }
        }
    }
}
