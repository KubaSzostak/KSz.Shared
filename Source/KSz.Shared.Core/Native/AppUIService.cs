using System;
using System.Collections.Generic;
using System.Text;

namespace System
{

    public interface IAppUIService
    {
        void DoEvents();
        void InitUnhandledExceptionHandler();
        void RestoreCursor();
        void SetWaitCursor();

        event StatusChangedEvent StatusChanged;
        void SetStatus(string info);

        event ProgressChangedEvent ProgressChanged;
        void SetProgress(double percent, string info);
        void SetProgressSteps(int count);
        void NextProgressStep(string info);
    }


    public abstract class AppUIService : IAppUIService
    {

        public abstract void DoEvents();
        public abstract void InitUnhandledExceptionHandler();
        public abstract void RestoreCursor();
        public abstract void SetWaitCursor();

        

        public event StatusChangedEvent StatusChanged;

        private string _lastInfo = null;
        public void SetStatus(string info)
        {
            if (info != _lastInfo)
            { 
                var sChanged = StatusChanged;
                sChanged?.Invoke(info);
                DoEvents();
            }
        }

        #region *** Progress ***

        private double lastPercent = 0;
        private DateTime lastUpdate = DateTime.Now;
        public event ProgressChangedEvent ProgressChanged;

        public void SetProgress(double percent, string info)
        {
            if ((Math.Abs(percent - lastPercent) > 1.0) || (percent <= 0.0) || (DateTime.Now - lastUpdate > TimeSpan.FromSeconds(0.3)))
            {
                var pChanged = this.ProgressChanged;
                pChanged?.Invoke(percent, info);
                SetWaitCursor();
                DoEvents();
                lastPercent = percent;
                lastUpdate = DateTime.Now;
            }
        }

        private int progressSteps = 100;
        public void SetProgressSteps(int count)
        {
            progressStep = 0;
            lastPercent = 0.0;
            progressSteps = count;
        }

        private int progressStep = 0;
        public void NextProgressStep(string info)
        {
            double p = 100 * (double)progressStep / (double)progressSteps;
            progressStep++;
            SetProgress(p, info);
        }


        #endregion
    }


    public static class AppUIEx
    {
        public static void ClearStatus(this IAppUIService appUi)
        {
            appUi.SetStatus(null);
            appUi.ClearProgress();
        }

        public static void ClearProgress(this IAppUIService appUi)
        {
            appUi.SetProgress(0.0, null);
            appUi.RestoreCursor();
            appUi.SetStatus(null);
            appUi.DoEvents();
        }
    }
}
