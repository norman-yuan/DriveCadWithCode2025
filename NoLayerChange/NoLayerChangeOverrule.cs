using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDb = Autodesk.AutoCAD.DatabaseServices;

namespace NoLayerChange
{
    public class NoLayerChangeOverrule : ObjectOverrule 
    {
        private bool _enabled = false;
        private const string FIXED_LAYER = "0";
        private readonly bool _origOverruling;

        private List<CadDb.ObjectId> _trackedEntities = new List<ObjectId>();

        public NoLayerChangeOverrule():base()
        {
            _origOverruling = Overruling;
        }

        public bool Enabled=>_enabled;

        public void Enable()
        {
            if (_enabled) return;

            _trackedEntities.Clear();

            AddOverrule(RXClass.GetClass(typeof(CadDb.Entity)), this, true);
            SetCustomFilter();
            _enabled = true;
            Overruling = true;
        }

        public void Disable()
        {
            if (!_enabled) return;

            RemoveOverrule(RXClass.GetClass(typeof(CadDb.Entity)), this);
            _enabled = false;
            Overruling = _origOverruling;
        }

        public override bool IsApplicable(RXObject overruledSubject)
        {
            var ent = overruledSubject as CadDb.Entity;
            if (ent == null) return false;

            return ent.Layer.ToUpper() == FIXED_LAYER;
        }

        public override void Open(DBObject dbObject, OpenMode mode)
        {
            if (!dbObject.IsNewObject)
            {
                var layer=((CadDb.Entity)dbObject).Layer;
                if (layer==FIXED_LAYER && !_trackedEntities.Contains(dbObject.ObjectId))
                {
                    _trackedEntities.Add(dbObject.ObjectId);
                }

                if (layer!=FIXED_LAYER )
                {

                }
            }
            base.Open(dbObject, mode);
        }

        public override void Close(DBObject dbObject)
        {
            RestoreLayer(dbObject);
            base.Close(dbObject);
        }

        private void RestoreLayer(DBObject dbObject)
        {
            if (!_trackedEntities.Contains(dbObject.ObjectId)) return;

            if (dbObject.IsUndoing) return;
            //if (!dbObject.IsModified) return;
            if (dbObject.IsErased) return;
            if (dbObject.IsNewObject) return;
            //if (!dbObject.IsWriteEnabled) return;
            

            var ent = dbObject as CadDb.Entity;
            if (ent == null) return;

            if (ent.Layer.ToUpper() != FIXED_LAYER)
            {
                if (!ent.IsWriteEnabled)
                {
                    ent.UpgradeOpen();
                }
                ent.Layer= FIXED_LAYER;
                _trackedEntities.Remove(dbObject.ObjectId);
            }
            
        }
    }
}
