using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DTest
{
    public class NetworkChangeTracker : ObjectOverrule
    {
        private readonly Document _dwg;
        private readonly List<string> _targetNetworks;
        private readonly bool _originalOverruling;

        private readonly List<ObjectId> _addedParts;
        private readonly List<ObjectId> _erasedParts;
        private readonly List<ObjectId> _modifiedParts;

        private bool _isEnabled = false;
        public NetworkChangeTracker(Document dwg)
        {
            _dwg = dwg;
            _targetNetworks = new List<string>();
            _originalOverruling = Overruling;
            _addedParts = new List<ObjectId>();
            _erasedParts = new List<ObjectId>();
            _modifiedParts = new List<ObjectId>();
        }

        public event NetworkChangeTracked NetworkChanged;

        public List<ObjectId> AddedParts=>_addedParts;
        public List<ObjectId> ModifiedParts => _modifiedParts;
        public List<ObjectId> ErasedParts => _erasedParts;
        public bool IsEnabled => _isEnabled;

        public void AddTrackingTarget(string networkName)
        {
            bool exist = false;
            foreach (var n in _targetNetworks)
            {
                if (n.Equals(networkName, StringComparison.OrdinalIgnoreCase))
                {
                    exist = true; 
                    break;
                }
            }
            if (!exist)
            {
                _targetNetworks.Add(networkName);
            }
        }

        public void RemoveTrackingTarget(string networkName)
        {
            foreach (var n in _targetNetworks)
            {
                if (!n.Equals(networkName, StringComparison.OrdinalIgnoreCase))
                {
                    _targetNetworks.Remove(n);
                    break;
                }
            }

            ResetTrackedParts();
        }

        public void ResetTrackedParts()
        {
            _addedParts.Clear();
            _erasedParts.Clear();
            _modifiedParts.Clear();
        }

        public void EnableTracling()
        {
            ResetTrackedParts();

            AddOverrule(GetClass(typeof(Part)), this, false);
            SetCustomFilter();
            Overruling = true;
            _isEnabled = true;
        }

        public void DisableTracking()
        {
            RemoveOverrule(GetClass(typeof(Part)), this);
            Overruling=_originalOverruling;
            _isEnabled = false;
        }

        public override bool IsApplicable(RXObject overruledSubject)
        {
            var part = overruledSubject as Part;
            if (part==null)
            {
                return false;
            }

            foreach (var network in _targetNetworks)
            {
                if (part.NetworkName.Equals(network, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public override void Close(Autodesk.AutoCAD.DatabaseServices.DBObject dbObject)
        {
            ObjectId id = dbObject.ObjectId;

            if (dbObject.IsNewObject)
            {
                if (!_addedParts.Contains(id))
                {
                    _addedParts.Add(id);
                    WriteMessage($"New part added: {id.Handle}");
                    NetworkChanged?.Invoke(this, new NetworkChangeEventArgs(_addedParts, _modifiedParts, _erasedParts));
                }
            }
            else if (dbObject.IsModified)
            {
                if (!_modifiedParts.Contains(id))
                {
                    _modifiedParts.Add(id);
                    WriteMessage($"Existing part modified: {id.Handle}");
                    NetworkChanged?.Invoke(this, new NetworkChangeEventArgs(_addedParts, _modifiedParts, _erasedParts));
                }
            }
            else if (dbObject.IsErased)
            {
                if (!_erasedParts.Contains(id))
                {
                    _erasedParts.Add(id);
                    WriteMessage($"Existing part erased: {id.Handle}");
                    NetworkChanged?.Invoke(this, new NetworkChangeEventArgs(_addedParts, _modifiedParts, _erasedParts));
                }
            }
            base.Close(dbObject);
        }

        private void WriteMessage(string message)
        {
            if (_dwg.IsActive)
            {
                _dwg.Editor.WriteMessage($"\n{message}");
            }
        }
    }

    public class NetworkChangeEventArgs : EventArgs
    {
        private readonly List<ObjectId> _addedParts;
        private readonly List<ObjectId> _modifiedParts;
        private readonly List<ObjectId> _erasedParts;

        public NetworkChangeEventArgs(List<ObjectId> addedParts, List<ObjectId> modifiedParts, List<ObjectId> erasedParts)
        {
            _addedParts = addedParts;
            _modifiedParts = modifiedParts;
            _erasedParts = erasedParts;
        }

        public List<ObjectId> AddedParts => _addedParts;
        public List<ObjectId> ModifiedParts => _modifiedParts;
        public List<ObjectId> ErasedParts => _erasedParts;
    }

    public delegate void NetworkChangeTracked(object sender, NetworkChangeEventArgs e);
}
