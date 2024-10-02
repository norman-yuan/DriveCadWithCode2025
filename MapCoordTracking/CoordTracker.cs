using Autodesk.Gis.Map;
using Autodesk.Gis.Map.Project;
using DriveCadWithCode2025.AutoCADUtilities;
using MapUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapCoordTracking
{
    public class CoordTracker
    {
        private readonly Document _dwg;
        private readonly ProjectModel _mapProject;
        private string _projectedCoord;
        private CoordTrackingViewModel _trackerViewModel = null;
        private CoordTrackingView _trackerView = null;
        private bool _wasVisible = false;

        public CoordTracker(Document dwg)
        {
            _dwg = dwg;
            _mapProject = HostMapApplicationServices.Application.Projects.GetProject(dwg);
            _projectedCoord = _mapProject.Projection;

            _mapProject.EndCSChange += (o, e) =>
            {
                _projectedCoord=_mapProject.Projection;
                if (_trackerViewModel!=null && _trackerView!=null)
                {
                    _trackerViewModel.SetProjectedCs(_projectedCoord);
                }
            };
        }

        #region public properties/methods

        public bool WasVisible => _wasVisible && _trackerView.Visibility == System.Windows.Visibility.Hidden && !_trackerViewModel.ManuallyClosed;
        public bool IsViewShown
        {
            get
            {
                if (_trackerView !=null)
                {
                    return _trackerView.Visibility == System.Windows.Visibility.Visible;
                }
                else
                {
                    return false;
                }
            }
        }
        public CoordTrackingView TrackerView => _trackerView;

        public void ShowView()
        {
            if (TrackerView == null)
            {
                _trackerViewModel = new CoordTrackingViewModel(_projectedCoord, CoordTrackSettings.TargetGeodeticCoords);
                _trackerView = new CoordTrackingView(_trackerViewModel);
                _trackerView.IsVisibleChanged += _trackerView_IsVisibleChanged;
            }

            _trackerViewModel.ManuallyClosed = false;
            CadApp.ShowModelessWindow(_trackerView);
        }

        public void AutoCloseView()
        {
            if (_trackerView == null) return;

            if (_trackerView.Visibility == System.Windows.Visibility.Visible)
            {
                _wasVisible = true;
            }
            _trackerView.Visibility = System.Windows.Visibility.Hidden;
        }

        public void ShutDownView()
        {
            _trackerView.ShutdownView();
        }

        #endregion

        #region private metnods

        private void _trackerView_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (_dwg == null) return;
            var win = (System.Windows.Window)sender;
            DisableCoordinateTracking();

            if (_dwg.IsActive && win.Visibility == System.Windows.Visibility.Visible)
            {
                EnableCoordinateTracking();
            }
        }

        private void EnableCoordinateTracking()
        {
            _dwg.Editor.PointMonitor += Editor_PointMonitor;
        }

        private void DisableCoordinateTracking()
        {
            try
            {
                _dwg.Editor.PointMonitor -= Editor_PointMonitor;
            }
            catch { }
        }

        private void Editor_PointMonitor(object sender, PointMonitorEventArgs e)
        {
            if (_trackerViewModel.CanShowCoordTracking)
            {
                var x = e.Context.RawPoint.X;
                var y = e.Context.RawPoint.Y;

                _trackerViewModel.Norting = y.ToString();
                _trackerViewModel.Easting = x.ToString();
            }
        }

        #endregion
    }
}
