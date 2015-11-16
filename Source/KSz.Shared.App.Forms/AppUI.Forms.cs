using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace System
{
    public partial class AppUI
    {
        static partial void GetProductNameNative(ref string appTitle)
        {
            appTitle = Application.ProductName;
        }

        static partial void GetProductVersionNative(ref string version)
        {
            version = Application.ProductName;
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

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                stream = dlg.OpenFile();
            }
        }

        static partial void SaveFileDialogNative(ref Stream stream, ref string filePath, string fileDescription, params string[] fileExtensions)
        {
            var dlg = new SaveFileDialog();
            InitFileDialog(dlg, filePath, fileDescription, fileExtensions);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                stream = dlg.OpenFile();
            }
        }



        static partial void ShowInfoNative(string message)
        {
            MessageBox.Show(message, AppUI.ProductName, MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        static partial void ShowWarningNative(string message)
        {
            MessageBox.Show(message, AppUI.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        static partial void ShowOKCancelNative(string message, ref bool result)
        {
            var dlgRes = MessageBox.Show(message, AppUI.ProductName, MessageBoxButtons.OKCancel);
            result = (dlgRes == DialogResult.OK);
        }

        static partial void ShowYesNoNative(string message, ref bool result)
        {
            var dlgRes = MessageBox.Show(message, AppUI.ProductName, MessageBoxButtons.YesNo);
            result = (dlgRes == DialogResult.Yes);
        }

        static partial void ShowYesNoCancelNative(string message, ref bool? result)
        {
            var dlgRes = MessageBox.Show(message, AppUI.ProductName, MessageBoxButtons.YesNoCancel);
            if (dlgRes == DialogResult.Yes)
                result = true;
            else if (dlgRes == DialogResult.No)
                result = false;
            else
                result = null;
        }

    }
}
