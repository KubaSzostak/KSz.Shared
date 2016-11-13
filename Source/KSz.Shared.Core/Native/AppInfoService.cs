using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;




namespace System
{

    public interface IAppInfoService
    {
        string AppName { get; }
        string AppVersion { get; }
    }
}
