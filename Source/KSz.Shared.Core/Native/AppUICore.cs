using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public partial class AppUI
    {
        public static string ProductName
        {
            get
            {
                var res = "Application";
                GetProductNameNative(ref res);
                return res;
            }
        }

        public static string ProductVersion
        {
            get
            {
                var res = "1.0.0.0";
                GetProductVersionNative(ref res);
                return res;
            }
        }

        public static void InitUnhandledExceptionHandler()
        {
            InitUnhandledExceptionHandlerNative();
        }







        public static void DoEvents()
        {
            DoEventsNative();
        }

        public static void SetWaitCursor()
        {
            SetWaitCursorNative();
        }

        public static void RestoreCursor()
        {
            RestoreCursorNative();
        }

        #region *** Status ***

        public static event StatusChangedEvent StatusChanged;

        public static void SetStatus(string info)
        {
            if (StatusChanged != null)
                StatusChanged.Invoke(info);
            DoEvents();
        }

        public static void ClearStatus()
        {
            SetStatus(null);
            ClearProgress();
        }

        #endregion

        #region *** Progress ***

        private static double lastPercent = 0;
        private static DateTime lastUpdate = DateTime.Now;
        public static event ProgressChangedEvent ProgressChanged;

        public static void SetProgress(double percent, string info)
        //public static void SetProgress(double percent, string info)
        {
            if ((Math.Abs(percent - lastPercent) > 1.0) || (percent < 0) || (DateTime.Now - lastUpdate > TimeSpan.FromSeconds(0.3)))
            {
                if (ProgressChanged != null)
                    ProgressChanged(percent, info); // ProgressChanged.Invoke(percent, info);
                SetWaitCursor();
                DoEvents();
                lastPercent = percent;
                lastUpdate = DateTime.Now;
            }
        }

        private static int progressSteps = 100;
        public static void SetProgressSteps(int count)
        {
            lastPercent = 0;
            progressSteps = count;
            progressStep = 0;
        }

        private static int progressStep = 0;
        public static void NextProgressStep(string info)
        {
            double p = 100 * (double)progressStep / (double)progressSteps;
            progressStep++;
            SetProgress(p, info);
        }

        public static void ClearProgress()
        {
            RestoreCursor();
            if (ProgressChanged != null)
                ProgressChanged(0.0, null);//.Invoke(0.0, null);
            lastPercent = 0;
            SetStatus(null);
            DoEvents();
        }

        #endregion

    }
}
