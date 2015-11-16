using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace System
{

    public partial class DialogServices
    {
        // There are many file dialogs, eg. OpenFileDialog exists in:
        //   WinForms   : Windows.Forms.OpenFileDialog
        //   WPF        : Microsoft.Win32.OpenFileDialog
        //   Silverlight: System.Windows.Controls.OpenFileDialog
        //   WinRT      : Windows.Storage.Pickers.FileOpenPicker (with quiet different filter approach)

        // And all of them are sealed... so you can't use interfaces

        // In all platforms you have access to Stream, but sometimes you need to know file path (eg. *.SHP)

        public Stream OpenFileDialog(ref string filePath, string fileDescription, params string[] fileExtensions)
        {
            Stream stream = null;
            OpenFileDialogNative(ref stream, ref filePath, fileDescription, fileExtensions);
            return stream;
        }

        public Stream SaveFileDialog(ref string filePath, string fileDescription, params string[] fileExtensions)
        {
            Stream stream = null;
            OpenFileDialogNative(ref stream, ref filePath, fileDescription, fileExtensions);
            return stream;
        }

        protected static string[] AddStarPrefixToExtension(string[] fileExtensions)
        {
            for (int i = 0; i < fileExtensions.Length; i++)
            {
                if (!fileExtensions[i].StartsWith("*"))
                    fileExtensions[i] = "*" + fileExtensions[i];
            }
            return fileExtensions;
        }

        protected static string GetFileFilter(string fileDescription, params string[] fileExtensions)
        {
            // Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*
            return fileDescription + "|" + fileExtensions.Join(";") + "|" + SysUtils.Strings.AllFiles + "|*.*";
        }

        public string OpenText(ref string filePath, string fileDescription, params string[] fileExtensions)
        {
            var stm = OpenFileDialog(ref filePath, fileDescription, fileExtensions);
            if (stm == null)
                return null;

            using (stm)
            {
                var reader = new StreamReader(stm, new UTF8Encoding(false));
                return reader.ReadToEnd();
            }
        }

        public bool SaveText(string text, ref string filePath, string fileDescription, params string[] fileExtensions)
        {
            var stm = SaveFileDialog(ref filePath, fileDescription, fileExtensions);
            if (stm == null)
                return false;

            using (stm)
            {
                var writer = new StreamWriter(stm, new UTF8Encoding(false));
                writer.Write(text);
                writer.Flush();
            }
            return true;
        }




        public void ShowInfo(string message)
        {
            ShowInfoNative(message);
        }

        public void ShowWarning(string message)
        {
            ShowWarningNative(message);
        }

        public bool ShowOKCancel(string message)
        {
            bool res = false;
            ShowOKCancelNative(message, ref res);
            return res;
        }

        public bool ShowYesNo(string message)
        {
            bool res = false;
            ShowYesNoNative(message, ref res);
            return res;
        }

        public bool? ShowYesNoCancel(string message)
        {
            bool? res = null;
            ShowYesNoCancelNative(message, ref res);
            return res;
        }

        public bool ConfirmDeleteItem(object item)
        {
            var msg = SysUtils.Strings.DeleteSelectedItemQ;
            if (item != null)
                msg += Environment.NewLine + item.ToString();
            return ShowYesNo(msg);
        }

    }
}
