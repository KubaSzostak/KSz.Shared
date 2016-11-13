using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace System
{

    public partial class AppServices
    {
        static partial void Init()
        {
            AppServices.Info = new AppInfoService();
            AppServices.Dialog = new DialogService();
            AppServices.OpenFileDialog = new OpenFileDialogService();
            AppServices.SaveFileDialog = new SaveFileDialogService();
        }
    }

    public class AppInfoService : IAppInfoService
    {
        public string AppName { get { return Application.ProductName; } }
        public string AppVersion { get { return Application.ProductVersion; } }
    }

    public class DialogService : IDialogService
    {
        public void ShowInfo(string message)
        {
            MessageBox.Show(message, AppServices.Info.AppName, MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        public bool ShowOKCancel(string message)
        {
            var dlgRes = MessageBox.Show(message, AppServices.Info.AppName, MessageBoxButtons.OKCancel);
            return (dlgRes == DialogResult.OK);
        }

        public void ShowWarning(string message)
        {
            MessageBox.Show(message, AppServices.Info.AppName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        public bool ShowYesNo(string message)
        {
            var dlgRes = MessageBox.Show(message, AppServices.Info.AppName, MessageBoxButtons.YesNo);
            return (dlgRes == DialogResult.Yes);
        }

        public bool? ShowYesNoCancel(string message)
        {
            var dlgRes = MessageBox.Show(message, AppServices.Info.AppName, MessageBoxButtons.YesNoCancel);
            if (dlgRes == DialogResult.Yes)
                return true;
            else if (dlgRes == DialogResult.No)
                return false;
            else
                return null;
        }
    }

    public abstract class FileDialogServiceNative : FileDialogService
    {

        protected IFileDialogAction ShowDialogNative(FileDialog dlg, Func<Stream> openFile)
        {
            this.AddStarPrefixToExtensions();
            dlg.Filter = this.GetFilter();
            dlg.InitialDirectory = IO.Path.GetDirectoryName(this.FilePath);
            dlg.FileName = IO.Path.GetFileNameWithoutExtension(this.FilePath);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using (var stream = openFile())
                {
                    if (stream == null)
                        return null;
                    return this.RunAction(dlg.FilterIndex - 1, stream, dlg.FileName);
                }
            }
            return null;
        }

    }

    public class OpenFileDialogService : FileDialogServiceNative
    {
        public override IFileDialogAction ShowDialog()
        {
            var dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            return ShowDialogNative(dlg, dlg.OpenFile);
        }
    }

    public class SaveFileDialogService : FileDialogServiceNative
    {
        public override IFileDialogAction ShowDialog()
        {
            var dlg = new SaveFileDialog();
            return ShowDialogNative(dlg, dlg.OpenFile);
        }
    }


}
