using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSP.GEO.UDS.WpfSupport;

namespace MapCoordTracking
{
    public class CoordTrackingViewModel : ViewModelBase
    {
        private string _projectedCs;
        private readonly string[] _geodeticCoords;
        private string _selectedGeodeticCoord = null;

        private bool _enabled = false;

        private string _norting = "";
        private string _easting = "";

        public CoordTrackingViewModel(string projectedCs, string[] geodeticCoords)
        {
            _projectedCs = projectedCs;
            _geodeticCoords = geodeticCoords;

            EnableTRackingCommand = new RelayCommand(
                EnableTracking, (o) => { return true; });
        }

        #region public properties

        public string ProjectedCoord => string.IsNullOrEmpty(_projectedCs) ? "No coordinate system assigned" : _projectedCs;
        public List<string> GeodeticCoords => _geodeticCoords.ToList();
        public bool ManuallyClosed {  get; set; }

        public string SelectedGeodeticCoord
        {
            get => _selectedGeodeticCoord;
            set
            {
                _selectedGeodeticCoord= value;
                OnPropertyChanged(nameof(SelectedGeodeticCoord));
                OnPropertyChanged(nameof(IsEnableButtonEnabled));
                OnPropertyChanged(nameof(EnableButtonText));
            }
        }

        public RelayCommand EnableTRackingCommand { get; private set; }

        public bool IsEnableButtonEnabled
        {
            get 
            { 
                return !string.IsNullOrEmpty(SelectedGeodeticCoord) && !string.IsNullOrEmpty(_projectedCs);
            }
        }
           
        public string EnableButtonText
        {
            get
            {
                if (IsEnableButtonEnabled)
                {
                    return _enabled ? "Disable" : "Enable";
                }
                else
                {
                    return "Enable";
                }
            }
        }

        public string Norting
        {
            get => _norting;
            set
            {
                _norting= value;
                OnPropertyChanged(nameof(Norting));
            }
        }

        public string Easting
        {
            get => _easting;
            set
            {
                _easting= value;
                OnPropertyChanged(nameof(Easting));
            }
        }

        public bool CanShowCoordTracking
        {
            get
            {
                if (string.IsNullOrEmpty(_projectedCs)) return false;
                if (string.IsNullOrEmpty(_selectedGeodeticCoord)) return false;
                if (!_enabled) return false;

                return true;
            }
        }

        public void SetProjectedCs(string projectedCs)
        {
            _projectedCs = projectedCs;
            OnPropertyChanged(nameof(ProjectedCoord));
        }

        #endregion

        #region private methods

        private void EnableTracking(object cmdParam)
        {
            _enabled = !_enabled;
            OnPropertyChanged(nameof(IsEnableButtonEnabled));
            OnPropertyChanged(nameof(EnableButtonText));
        }

        #endregion
    }
}
