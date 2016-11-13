using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace System
{

    class AppLogService : IAppLogService
    {
        private static readonly string _logFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Logs\";
        private static readonly string _logFile = _logFolder + Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]) + "-log.txt";
        private static readonly int _logSize = 1024 * 1024;

        public AppLogService()
        {
            Directory.CreateDirectory(_logFolder);
            TrimToSize(_logSize);
        }

        public string LogFilePath { get { return _logFile; } }

        private static void TrimToSize(int size)
        {
            if (File.Exists(_logFile) && (new FileInfo(_logFile).Length > size))
            {

                var fData = File.ReadAllText(_logFile);
                int pos = (int)fData.Length - size;
                if (pos > 0)
                {
                    fData = fData.Substring(pos);
                    var lnBreakPos = fData.LastIndexOf("\r");
                    if (lnBreakPos < 1)
                        lnBreakPos = fData.LastIndexOf("n");
                    if (lnBreakPos > 0)
                    {
                        fData = fData.Substring(0, lnBreakPos + 1);
                    }
                    File.WriteAllText(_logFile, fData);
                }
            }
        }

        public void Add(DateTime? time, string text)
        {
            try
            {
                if (time.HasValue)
                    text = time.Value.ToString() + ": " + text;
                text = "\r\n" + text;
                System.IO.File.AppendAllText(_logFile, text);
            }
            catch (Exception)
            {
            }
        }

        public static string LogText
        {
            get { return File.ReadAllText(_logFile); }
        }

        public static string[] LogLines
        {
            get { return File.ReadAllLines(_logFile); }
        }
    }
}