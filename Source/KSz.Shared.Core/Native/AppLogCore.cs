using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace System
{
    public partial class AppLog
    {

        public static void AddLn(DateTime? time, string msg)
        {
            AddLnNative(time, msg);
        }

        public static void Add(string msg)
        {
            AddLn(DateTime.Now, msg);
        }

        public static void Add(string msg, params string[] args)
        {
            Add(string.Format(msg, args));
        }

        public static void Add(Exception ex)
        {
            Add(ex.GetExtendedMessage(null, null, true));
        }

        public static void StartSectionLn()
        {
            AddLn(null, "");
            AddLn(null, "---------------------------------------------------------------------");
        }
    }
}
