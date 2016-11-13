using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public partial class AppServices
    {
        static partial void Init()
        {
            AppServices.Dialog = new WpfDialogServices();
            AppServices.Info = new WpfAppInfoService();
            AppServices.Log = new AppLogService();
            AppServices.OpenFileDialog = new WpfOpenFileDialogService();
            AppServices.SaveFileDialog = new WpfSaveFileDialogService();
            AppServices.UI = new WpfAppUIService();
        }
    }
}
