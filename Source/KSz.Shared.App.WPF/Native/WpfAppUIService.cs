using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace System
{
    class WpfAppUIService : AppUIService
    {
        public override void DoEvents()
        {
            var app = Application.Current;
            if (app == null || app.Dispatcher == null || app.MainWindow == null)
                return;
            if (!app.MainWindow.IsLoaded || !app.MainWindow.IsActive || !app.MainWindow.IsVisible)
                return;
            //app.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
            app.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new ThreadStart(delegate { }));
        }

        public override void InitUnhandledExceptionHandler()
        {
            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(OnUnhandledException);

            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException); // All threads in AppDomain
            // Dispatcher.UnhandledException // Single, specifiic UI Dispatcher thread
            // Application.DispatcherUnhandledException // From the Main UI dispatcher thread in your WPF
        }

        private static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //ReportUnhandledException(e.Exception);
            //throw e.Exception;
            AppServices.Dialog.ShowWarning(e.Exception.Message);
            e.Handled = true;
        }

        public override void RestoreCursor()
        {
            Mouse.OverrideCursor = null;
        }

        public override void SetWaitCursor()
        {
            Mouse.OverrideCursor = Cursors.Wait;
        }
    }
}
