using System;
using System.Collections.Generic;
using System.Text;

namespace System
{

    public interface IAppLogService
    {
        void Add(DateTime? time, string text);
        string LogFilePath { get; }

    }



    public static class AppLogServiceEx
    {

        public static void Add(this IAppLogService appLog, string msg)
        {
            appLog.Add(DateTime.Now, msg);
        }

        public static void Add(this IAppLogService appLog, string msg, params string[] args)
        {
            appLog.Add(string.Format(msg, args));
        }

        public static void Add(this IAppLogService appLog, Exception ex)
        {
            appLog.Add(ex.GetExtendedMessage(null, null, true));
        }

        public static void NewSection(this IAppLogService appLog)
        {
            appLog.Add(null, "");
            appLog.Add(null, "---------------------------------------------------------------------");
        }

        public static void AddLn(this IAppLogService appLog)
        {
            appLog.Add(null, "");
        }
    }
}