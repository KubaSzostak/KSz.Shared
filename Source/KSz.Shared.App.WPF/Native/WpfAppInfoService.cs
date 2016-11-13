using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Markup;

[assembly: XmlnsPrefix("http://kubaszostak.github.io/", "ksz")]
[assembly: XmlnsDefinition("http://kubaszostak.github.io/", "System")]
[assembly: XmlnsDefinition("http://kubaszostak.github.io/", "System.IO")]


namespace System
{
    class WpfAppInfoService : IAppInfoService
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

        private static FileVersionInfo _entryAssemblyInfo = null;
        private static FileVersionInfo EntryAssemblyInfo
        {
            get
            {
                if (_entryAssemblyInfo == null)
                {
                    var assembly = Assembly.GetEntryAssembly();
                    _entryAssemblyInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                }
                return _entryAssemblyInfo;
            }
        }

        public string AppName
        {
            get
            {
                return EntryAssemblyInfo.ProductName; //Eg. BricsCAD, not Ksz.App.WPF

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
        }

        public string AppVersion
        {
            get { return ExecutingAssemblyInfo.ProductVersion; } //Version from Ksz.App.WPF, not from BricsCAD application 
        }

    }
}
