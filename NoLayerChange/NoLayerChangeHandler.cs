using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDb = Autodesk.AutoCAD.DatabaseServices;

namespace NoLayerChange
{
    public class NoLayerChangeHandler
    {
        private readonly Document _dwg;
        private readonly List<CadDb.ObjectId> _trackedEntities;
        private readonly List<CadDb.ObjectId> _layerChangedEntities;
        private const string FIXED_LAYER = "0";
        private bool _isEnabled = false;
        private bool _idleHandled = false;

        public NoLayerChangeHandler(Document dwg)
        {
            _dwg = dwg;
            _trackedEntities = new List<CadDb.ObjectId>();
            _layerChangedEntities = new List<CadDb.ObjectId>();
        }

        public bool IsEnabled=> _isEnabled;

        public void Enable()
        {
            if (_isEnabled) return;

            HookEventHandler(true);
            _isEnabled = true;
        }

        public void Disable()
        {
            if (!_isEnabled) return;

            HookEventHandler(false);
            _isEnabled = false;
        }

        private void HookEventHandler(bool hook)
        {
            if (hook)
            {
                _dwg.Database.ObjectOpenedForModify += Database_ObjectOpenedForModify;
                _dwg.Database.ObjectModified += Database_ObjectModified;
            }
            else
            {
                _dwg.Database.ObjectOpenedForModify -= Database_ObjectOpenedForModify;
                _dwg.Database.ObjectModified -= Database_ObjectModified;
            }
        }

        private void Database_ObjectModified(object sender, ObjectEventArgs e)
        {
            var ent = e.DBObject as Entity;
            if (ent == null) return;
            if (!_trackedEntities.Contains(ent.ObjectId)) return;

            if (ent.Layer.ToUpper() != FIXED_LAYER)
            {
                _layerChangedEntities.Add(ent.ObjectId);
                if (!_idleHandled)
                {
                    _idleHandled = true;
                    CadApp.Idle += CadApp_Idle;
                }
            }
        }

        private void CadApp_Idle(object? sender, EventArgs e)
        {
            CadApp.Idle -= CadApp_Idle;
            if (_layerChangedEntities.Count == 0) return;

            Disable();

            try
            {
                using (_dwg.LockDocument())
                {
                    using (var tran = _dwg.Database.TransactionManager.StartTransaction())
                    {
                        foreach (var id in _layerChangedEntities)
                        {
                            var ent = (CadDb.Entity)tran.GetObject(id, OpenMode.ForWrite);
                            if (ent.Layer.ToUpper() != FIXED_LAYER)
                            {
                                ent.Layer = FIXED_LAYER;
                            }
                        }
                        tran.Commit();
                    }
                }

                _dwg.Editor.WriteMessage(
                    $"\nFeature entity layer changes have been denied: {_layerChangedEntities.Count}.");
            }
            finally
            {
                _idleHandled = false;
                _layerChangedEntities.Clear();
                _trackedEntities.Clear();
                Enable();
            }
        }

        private void Database_ObjectOpenedForModify(object sender, ObjectEventArgs e)
        {
            var ent = e.DBObject as Entity;
            if (ent == null) return;
            if (ent.Layer.ToUpper()==FIXED_LAYER && !_trackedEntities.Contains(ent.ObjectId))
            {
                _trackedEntities.Add(ent.ObjectId);
            }
        }

        private void RemoveEventHandler()
        {
            _dwg.Database.ObjectOpenedForModify -= Database_ObjectOpenedForModify;
            _dwg.Database.ObjectModified -= Database_ObjectModified;
        }
    }
}
