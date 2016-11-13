using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace System
{
    class WpfDialogServices : IDialogService
    {
        public void ShowInfo(string message)
        {
            MessageBox.Show(message, AppServices.Info.AppName, MessageBoxButton.OK, MessageBoxImage.None);
        }

        public bool ShowOKCancel(string message)
        {
            var dlgRes = MessageBox.Show(message, AppServices.Info.AppName, MessageBoxButton.OKCancel);
            return (dlgRes == MessageBoxResult.OK);
        }

        public void ShowWarning(string message)
        {
            MessageBox.Show(message, AppServices.Info.AppName, MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        public bool ShowYesNo(string message)
        {
            var dlgRes = MessageBox.Show(message, AppServices.Info.AppName, MessageBoxButton.YesNo);
            return (dlgRes == MessageBoxResult.Yes);
        }

        public bool? ShowYesNoCancel(string message)
        {
            var dlgRes = MessageBox.Show(message, AppServices.Info.AppName, MessageBoxButton.YesNoCancel);
            if (dlgRes == MessageBoxResult.Yes)
                return true;
            else if (dlgRes == MessageBoxResult.No)
                return false;
            else
                return null;
        }
    }
}
