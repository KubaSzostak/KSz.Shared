using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;


[assembly: XmlnsPrefix("http://kubaszostak.github.io/", "ksz")]
[assembly: XmlnsDefinition("http://kubaszostak.github.io/", "KSz.Controls")]

namespace System
{
    public partial class AppUI
    {

        private static FileVersionInfo _executingAssemblyInfo = null;
        private static FileVersionInfo ExecutingAssemblyInfo
        {
            get
            {
                if (_executingAssemblyInfo == null)
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    _executingAssemblyInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                }
                return _executingAssemblyInfo;
            }
        }

        static partial void GetProductNameNative(ref string appTitle)
        {
            appTitle = ExecutingAssemblyInfo.ProductName;

            //var companyName = fvi.CompanyName;
            //var productNAme = fvi.ProductName;
            //var productVersion = fvi.ProductVersion;

            //if (Application.Current != null)
            //{
            //    if (Application.Current.MainWindow != null)
            //        this.Text = Application.Current.MainWindow.Title;
            //     else
            //        this.Text = Application.Current.ToString();
            //}



            //if ((Application.Current == null) || (Application.Current.MainWindow == null))
            //    return AppDomain.CurrentDomain.FriendlyName;
            //return Application.Current.MainWindow.Title;

        }

        static partial void GetProductVersionNative(ref string version)
        {
            version = ExecutingAssemblyInfo.ProductVersion;
        }


        static partial void SetWaitCursorNative()
        {
            Mouse.OverrideCursor = Cursors.Wait;
        }

        static partial void RestoreCursorNative()
        {
            Mouse.OverrideCursor = null;
        }

        static partial void DoEventsNative()
        {
            var app = Application.Current;
            if (app == null || app.Dispatcher == null || app.MainWindow == null)
                return;
            if (!app.MainWindow.IsLoaded || !app.MainWindow.IsActive || !app.MainWindow.IsVisible)
                return;
            //app.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
            app.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new ThreadStart(delegate { }));
        }


        static partial void InitUnhandledExceptionHandlerNative()
        {
            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(OnUnhandledException);

            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException); // All threads in AppDomain
            // Dispatcher.UnhandledException // Single, specifiic UI Dispatcher thread
            // Application.DispatcherUnhandledException // From the Main UI dispatcher thread in your WPF
        }

        private static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ReportUnhandledException(e.Exception);
            //throw e.Exception;
            AppUI.Dialog.ShowWarning(e.Exception.Message);
            e.Handled = true;
        }
    }


    public partial class DialogServices
    {
        private static void InitFileDialog(FileDialog dlg, string filePath, string fileDescription, params string[] fileExtensions)
        {
            fileExtensions = AddStarPrefixToExtension(fileExtensions);
            dlg.Filter = GetFileFilter(fileDescription, fileExtensions);
            dlg.InitialDirectory = IO.Path.GetDirectoryName(filePath);
            dlg.FileName = IO.Path.GetFileNameWithoutExtension(filePath);
        }

        static partial void OpenFileDialogNative(ref Stream stream, ref string filePath, string fileDescription, params string[] fileExtensions)
        {
            var dlg = new OpenFileDialog();
            InitFileDialog(dlg, filePath, fileDescription, fileExtensions);
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == true)
            {
                stream = dlg.OpenFile();
            }
        }

        static partial void SaveFileDialogNative(ref Stream stream, ref string filePath, string fileDescription, params string[] fileExtensions)
        {
            var dlg = new SaveFileDialog();
            InitFileDialog(dlg, filePath, fileDescription, fileExtensions);

            if (dlg.ShowDialog() == true)
            {
                stream = dlg.OpenFile();
            }
        }



        static partial void ShowInfoNative(string message)
        {
            MessageBox.Show(message, AppUI.ProductName, MessageBoxButton.OK, MessageBoxImage.None);
        }

        static partial void ShowWarningNative(string message)
        {
            MessageBox.Show(message, AppUI.ProductName, MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        static partial void ShowOKCancelNative(string message, ref bool result)
        {
            var dlgRes = MessageBox.Show(message, AppUI.ProductName, MessageBoxButton.OKCancel);
            result = (dlgRes == MessageBoxResult.OK);
        }

        static partial void ShowYesNoNative(string message, ref bool result)
        {
            var dlgRes = MessageBox.Show(message, AppUI.ProductName, MessageBoxButton.YesNo);
            result = (dlgRes == MessageBoxResult.Yes);
        }
   
        static partial void ShowYesNoCancelNative(string message, ref bool? result)
        {
            var dlgRes = MessageBox.Show(message, AppUI.ProductName, MessageBoxButton.YesNoCancel);
            if (dlgRes == MessageBoxResult.Yes)
                result = true;
            else if (dlgRes == MessageBoxResult.No)
                result = false;
            else
                result = null;
        }

    }


}
