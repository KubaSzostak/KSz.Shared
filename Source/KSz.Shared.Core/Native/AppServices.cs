using System;
using System.Collections.Generic;
using System.Text;

namespace System
{

    public partial class AppServices
    {
        static partial void Init();

        public static IAppInfoService Info { get; private set; }
        public static IAppLogService Log { get; private set; }
        public static IAppUIService UI { get; private set; }
        public static IDialogService Dialog { get; private set; }
        public static IFileDialogService OpenFileDialog { get; private set; }
        public static IFileDialogService SaveFileDialog { get; private set; }
        public static LocalizationStrings Strings { get; private set; }

        static AppServices()
        {
            AppServices.Strings = new LocalizationStrings();
            Init();
        }
    }
}
