
[assembly: ExtensionApplication(typeof(MapCoordTracking.MyCommands))]
[assembly: CommandClass(typeof(MapCoordTracking.MyCommands))]

namespace MapCoordTracking
{
    public class MyCommands : IExtensionApplication
    {
        #region IExtensionApplication implemetation

        public void Initialize()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            if (dwg != null)
            {
                var ed = dwg.Editor;
                ed.WriteMessage("\nInitializing custom plugin...");
                try
                {
                    //Do something when neceeary

                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\nInitializing error:\n{ex.Message}");
                }
            }

            //add Idle event handler, if necessary, to do something AFTER AutoCAD startup compelete
        }

        public void Terminate()
        {

        }

        #endregion

        [CommandMethod("TrackCoord", CommandFlags.Session)]
        public static void RunMyCommand()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            CoordTrackerCollection.Instance.ShowTracker(dwg);
        }
    }
}
