using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapCoordTracking
{
    /// <summary>
    /// Interaction logic for CoordTrackingView.xaml
    /// </summary>
    public partial class CoordTrackingView : Window
    {
        private CoordTrackingViewModel _viewModel = null;
        private bool _closeRequested = false;
        public CoordTrackingView()
        {
            InitializeComponent();
        }

        public CoordTrackingView(CoordTrackingViewModel viewModel) : this()
        {
            _viewModel = viewModel;
            Loaded += (o, e) =>
            {
                DataContext = _viewModel;
            };
        }

        public void ShutdownView()
        {
            _closeRequested = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_closeRequested)
            {
                e.Cancel = true;
                _viewModel.ManuallyClosed = true;
                Visibility = System.Windows.Visibility.Hidden;
            }
        }

    }
}
