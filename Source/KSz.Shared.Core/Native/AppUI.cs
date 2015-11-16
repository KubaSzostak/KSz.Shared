using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

    


namespace System
{

    // Interface for native implemenation
    

    public partial class AppUI
    {
        static partial void GetProductNameNative(ref string appTitle);
        static partial void GetProductVersionNative(ref string version);
        static partial void SetWaitCursorNative();
        static partial void RestoreCursorNative();
        static partial void DoEventsNative();
        static partial void InitUnhandledExceptionHandlerNative();

        static partial void ReportUnhandledException(Exception exception);


        public static DialogServices Dialog = new DialogServices();



    }

    public partial class DialogServices
    {
        static partial void OpenFileDialogNative(ref Stream stream, ref string filePath, string fileDescription, params string[] fileExtensions);
        static partial void SaveFileDialogNative(ref Stream stream, ref string filePath, string fileDescription, params string[] fileExtensions);

        static partial void ShowInfoNative(string message);
        static partial void ShowWarningNative(string message);
        static partial void ShowOKCancelNative(string message, ref bool result);
        static partial void ShowYesNoNative(string message, ref bool result);
        static partial void ShowYesNoCancelNative(string message, ref bool? result);        
    }
    


}
