
[assembly: ExtensionApplication(typeof(CoreConsolePlugin.MyCommands))]
[assembly: CommandClass(typeof(CoreConsolePlugin.MyCommands))]

namespace CoreConsolePlugin
{
    public class MyCommands : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }

        [CommandMethod("HelloCoreConsole")]
        public static void RunMyCommand()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            if (dwg != null)
            {
                var ed = dwg.Editor;
                ed.WriteMessage("\nHello from AutoCAD 2025");
                ed.WriteMessage($"\nIs in-place server: {CadApp.IsInPlaceServer}");
                ed.WriteMessage($"\nIs in background mode: {CadApp.IsInBackgroundMode}");
            }
        }
    }
}
