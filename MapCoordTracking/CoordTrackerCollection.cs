using DriveCadWithCode2025.AutoCADUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapCoordTracking
{
    public class CoordTrackerCollection : Dictionary<IntPtr, CoordTracker>
    {
        private static CoordTrackerCollection _instance = null;
        private CoordTrackerCollection()
        {
            CadApp.DocumentManager.DocumentBecameCurrent += DocumentManager_DocumentBecameCurrent;
            CadApp.DocumentManager.DocumentToBeDestroyed += DocumentManager_DocumentToBeDestroyed;
        }

        private void DocumentManager_DocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            var doc = e.Document;
            if (ContainsKey(doc.UnmanagedObject))
            {
                var tracker = this[doc.UnmanagedObject];
                Remove(doc.UnmanagedObject);
                tracker.ShutDownView();
                tracker = null;
            }
        }

        private void DocumentManager_DocumentBecameCurrent(object sender, DocumentCollectionEventArgs e)
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            if (dwg == null) return;

            if (ContainsKey(dwg.UnmanagedObject))
            {
                var tracker = this[dwg.UnmanagedObject];
                if (tracker.WasVisible)
                {
                    tracker.ShowView();
                }
            }

            foreach (Document d in CadApp.DocumentManager)
            {
                if (d.UnmanagedObject !=dwg.UnmanagedObject)
                {
                    if (ContainsKey(d.UnmanagedObject))
                    {
                        var tracker = this[d.UnmanagedObject];
                        if (tracker.IsViewShown)
                        {
                            tracker.AutoCloseView();
                        }
                    }
                }
            }
        }

        public static CoordTrackerCollection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance= new CoordTrackerCollection();
                }
                return _instance;
            }
        }

        public void ShowTracker(Document dwg)
        {
            CoordTracker tracker = null;
            if (ContainsKey(dwg.UnmanagedObject))
            {
                tracker= this[dwg.UnmanagedObject];
            }
            else
            {
                tracker=new CoordTracker(dwg);
                Add(dwg.UnmanagedObject,tracker);
            }

            tracker.ShowView();
        }
    }
}
