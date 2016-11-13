using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace System
{


    public interface IDialogService
    {
        void ShowInfo(string message);
        void ShowWarning(string message);
        bool ShowOKCancel(string message);
        bool ShowYesNo(string message);
        bool? ShowYesNoCancel(string message);

    }

    public static class DialogServiceEx 
    {
        public static bool ConfirmDeleteItem(this IDialogService dlg, object item)
        {
            var msg = SysUtils.Strings.DeleteSelectedItemQ;
            if (item != null)
                msg += Environment.NewLine + item.ToString();
            return dlg.ShowYesNo(msg);
        }
    }

}
