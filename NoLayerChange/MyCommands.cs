
using CadDb = Autodesk.AutoCAD.DatabaseServices;
[assembly: ExtensionApplication(typeof(NoLayerChange.MyCommands))]
[assembly: CommandClass(typeof(NoLayerChange.MyCommands))]

namespace NoLayerChange
{
    public class MyCommands : IExtensionApplication
    {
        public void Initialize()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            if (dwg != null)
            {
                var ed = dwg.Editor;
                ed.WriteMessage("\nInitializing custom plugin: NoLayerChange, ...");
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

        private NoLayerChangeOverrule? _overrule;
        [CommandMethod("NoLayerChange")]
        public void RunMyCommand()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            if (_overrule==null)
            {
                _overrule= new NoLayerChangeOverrule();
            }

            if (!_overrule.Enabled)
            {
                _overrule.Enable();
                ed.WriteMessage("\nNoLayerChange Overrule is enabled.\n");
            }
            else
            {
                _overrule.Disable();
                ed.WriteMessage("\nNoLayerChange Overrule is disabled.\n");
            }
        }

        private NoLayerChangeHandler? _layerHandler;
        [CommandMethod("LayerChangeHandler")]
        public void RunLayerNoChangeHandler()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            if (_layerHandler == null)
            {
                _layerHandler = new NoLayerChangeHandler(dwg);
            }

            if (!_layerHandler.IsEnabled)
            {
                _layerHandler.Enable();
                ed.WriteMessage("\nNoLayerChange Handler is enabled.\n");
            }
            else
            {
                _layerHandler.Disable();
                ed.WriteMessage("\nNoLayerChange Handler is disabled.\n");
            }
        }

        [CommandMethod("SetLayer")]
        public void ChangeLayer()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var res = ed.GetEntity("\nSelect an entity:");
            if (res.Status== PromptStatus.OK)
            {
                using (var tran = dwg.TransactionManager.StartTransaction())
                {
                    var ent = (CadDb.Entity)tran.GetObject(res.ObjectId, OpenMode.ForWrite);
                    if (ent.Layer=="0")
                    {
                        ent.Layer = "Layer1";
                    }
                    tran.Commit();
                }
            }
        }
    }
}
