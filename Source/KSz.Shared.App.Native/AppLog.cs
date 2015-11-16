using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace System
{
    public partial class AppLog
    {
        public static readonly string LogFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Logs\";
        public static readonly string LogFile = LogFolder + Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]) + "-log.txt";
        
        static AppLog()
        {
            Directory.CreateDirectory(LogFolder);
            TrimToSize(1024 * 1024);
        }

        static partial void AddLnNative(DateTime? time, string msg)
        {
            try
            {
                if (time.HasValue)
                    msg = time.Value.ToString() + ": " + msg;
                msg = "\r\n" + msg;
                System.IO.File.AppendAllText(LogFile, msg);
            }
            catch (Exception)
            {
            }
        }

        private static void TrimToSize(int size)
        {
            if (File.Exists(LogFile) && (new FileInfo(LogFile).Length > size))
            {
                var fData = File.ReadAllText(LogFile);
                int pos = (int)fData.Length - size;
                if (pos > 0)
                {
                    fData = fData.Substring(pos);
                    File.WriteAllText(LogFile, fData);
                }

            }
        }

        public static string LogText
        {
            get { return File.ReadAllText(LogFile); }
        }

        public static string[] LogLines
        {
            get { return File.ReadAllLines(LogFile); }
        }
    }
}