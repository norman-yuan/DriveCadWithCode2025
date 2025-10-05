using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadMicsTests
{
    public class EntityManualSelector
    {
        private readonly Editor? _ed;
        private readonly Document _dwg;
        private ObjectId[]? _selectedIds = null;
        private bool _layerOption = true;

        public EntityManualSelector(Document dwg)
        {
            _dwg = dwg;
            _ed = dwg.Editor;
        }
        public IEnumerable<ObjectId> SelectedIds => _selectedIds;

        public bool SelectEntities()
        {
            var handlerAdded = false;

            try
            {
                while (true)
                {
                    var go = true;
                    if (_layerOption)
                    {
                        _ed.SelectionAdded += Editor_SelectionAdded;
                        _ed.SelectionRemoved += Editor_SelectionRemoved;
                        handlerAdded = true;
                    }
                    var opt = new PromptSelectionOptions();
                    if (_layerOption)
                    {
                        opt.Keywords.Add("Enable selection of entities on the Same layer");
                        opt.Keywords.Default = "Enable";
                    }
                    else
                    {
                        opt.Keywords.Add("Disable selection of entities on the Same layer");
                        opt.Keywords.Default = "Disable";
                    }

                    var res = _ed.GetSelection(opt);
                    if (res.Status== PromptStatus.OK)
                    {
                        _selectedIds = res.Value.GetObjectIds();
                        go = false;
                    }
                    else if (res.Status== PromptStatus.Keyword)
                    {
                        go = true;
                    }
                    else
                    {
                        _selectedIds = null;
                        go = false;
                    }

                    if (handlerAdded)
                    {
                        _ed.SelectionAdded -= Editor_SelectionAdded;
                        _ed.SelectionRemoved -= Editor_SelectionRemoved;
                        handlerAdded = false;
                    }

                    if (!go)
                    {
                        break;
                    }
                }
            }
            finally
            {

            }
            return _selectedIds != null;
        }

        private void Editor_SelectionRemoved(object sender, SelectionRemovedEventArgs e)
        {
            var layerIds = GetRelatedLayers(e.RemovedObjects.GetObjectIds());
        }

        private void Editor_SelectionAdded(object sender, SelectionAddedEventArgs e)
        {
            var layerIds = GetRelatedLayers(e.AddedObjects.GetObjectIds());
            
        }

        private List<ObjectId> GetRelatedLayers(IEnumerable<ObjectId> objectIds)
        {
            var layerIds = new List<ObjectId>();

            using (var tran = _dwg.Database.TransactionManager.StartTransaction())
            {
                foreach (var id in objectIds)
                {
                    var ent = (Entity)tran.GetObject(id, OpenMode.ForRead);
                    if (!layerIds.Contains(ent.LayerId))
                    {
                        layerIds.Add(ent.LayerId);
                    }
                }
                tran.Commit();
            }
            return layerIds;
        }

        private List<ObjectId> FindEntitiesOnLayers(List<ObjectId> layerIds)
        {
            var entities = new List<ObjectId>();

            return entities;
        }
    }
}
