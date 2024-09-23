using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WSP.GEO.UDS.WpfSupport
{
    public class MyMessageBox
    {
        private string _title;
        public MyMessageBox(string title)
        {
            _title = title;
        }

        public MessageBoxResult ShowInfo(string message)
        {
            return MessageBox.Show(message, _title,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public MessageBoxResult ShowError(string message)
        {
            return MessageBox.Show(message, _title,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public MessageBoxResult ShowExclamation(string message)
        {
            return MessageBox.Show(message, _title,
                MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        public MessageBoxResult ShowYesNo_DefaultYes(string message, bool showAsWarning=false)
        {
            return MessageBox.Show(message, _title,
                MessageBoxButton.YesNo,
                showAsWarning ? MessageBoxImage.Warning : MessageBoxImage.Question,
                MessageBoxResult.Yes);
        }

        public MessageBoxResult ShowYesNo_DefaultNo(string message, bool showAsWarning = false)
        {
            return MessageBox.Show(message, _title,
                MessageBoxButton.YesNo,
                showAsWarning ? MessageBoxImage.Warning : MessageBoxImage.Question,
                MessageBoxResult.No);
        }

        public MessageBoxResult ShowYesNoCancel_DefaultYes(string message, bool showAsWarning = false)
        {
            return MessageBox.Show(message, _title,
                MessageBoxButton.YesNoCancel,
                showAsWarning ? MessageBoxImage.Warning : MessageBoxImage.Question,
                MessageBoxResult.Yes);
        }

        public MessageBoxResult ShowYesNoCancel_DefaultNo(string message, bool showAsWarning = false)
        {
            return MessageBox.Show(message, _title,
                MessageBoxButton.YesNoCancel,
                showAsWarning ? MessageBoxImage.Warning : MessageBoxImage.Question,
                MessageBoxResult.No);
        }

        public MessageBoxResult ShowYesNoCancel_DefaultCancel(string message, bool showAsWarning = false)
        {
            return MessageBox.Show(message, _title,
                MessageBoxButton.YesNoCancel,
                showAsWarning ? MessageBoxImage.Warning : MessageBoxImage.Question,
                MessageBoxResult.Cancel);
        }
    }
}
