using Autodesk.AutoCAD.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveCadWithCode2025.AutoCADUtilities
{
    public interface IDialog
    {
        object DataContext { get; set; }
        bool? DialogResult { get; set; }
        void Close();
    }

    public interface IDialogRequestClose
    {
        event EventHandler<DialogRequestCloseEventArgs> CloseRequested;
        System.Windows.Window View { set; get; }
    }

    public class DialogRequestCloseEventArgs : EventArgs
    {
        public DialogRequestCloseEventArgs(bool? dialogResult)
        {
            DialogResult = dialogResult;
        }

        public bool? DialogResult { get; }
    }

    public interface IDialogService
    {
        void Register<TViewModel, TView>() where TViewModel : IDialogRequestClose where TView : IDialog;
        bool? ShowDialog<TViewModel>(TViewModel viewModel, bool referenceToView = false) where TViewModel : IDialogRequestClose;
    }

    public class WpfDialogService : IDialogService
    {
        public IDictionary<Type, Type> ViewMappings { get; } = new Dictionary<Type, Type>();

        public void Register<TViewModel, TView>()
            where TViewModel : IDialogRequestClose
            where TView : IDialog
        {
            if (ViewMappings.ContainsKey(typeof(TViewModel)))
            {
                return;
            }

            ViewMappings.Add(typeof(TViewModel), typeof(TView));
        }

        public bool? ShowDialog<TViewModel>(TViewModel viewModel, bool referenceToView = false)
            where TViewModel : IDialogRequestClose
        {
            Type viewType = ViewMappings[typeof(TViewModel)];
            IDialog dialog = null;
            System.Windows.Window view = null;
            try
            {
                view = viewModel.View;
                if (view != null)
                {
                    view.Close();
                    viewModel.View = null;
                    view = null;
                }
            }
            catch
            {
            }

            dialog = (IDialog)Activator.CreateInstance(viewType);
            view = dialog as System.Windows.Window;
            if (referenceToView)
            {
                viewModel.View = view;
            }

            EventHandler<DialogRequestCloseEventArgs> handler = null;
            handler = (sender, e) =>
            {
                viewModel.CloseRequested -= handler;
                if (viewModel.View != null)
                {
                    dialog = (IDialog)viewModel.View;
                }

                if (((System.Windows.Window)dialog).IsVisible)
                {
                    if (e.DialogResult.HasValue)
                    {
                        dialog.DialogResult = e.DialogResult;
                    }
                }

                dialog.Close();
                viewModel.View = null;
            };

            viewModel.CloseRequested += handler;

            dialog.DataContext = viewModel;

            return CadApp.ShowModalWindow(CadApp.MainWindow.Handle, dialog as System.Windows.Window, false);
        }
    }
}
