using System;
using System.Collections.Generic;
using System.Text;

namespace System
{

    public static class ExceptionExtensions
    {
        private const string TraceInfoKey = "KSz.TraceInfo";

        public static string GetAllMessages(this Exception ex, int maxInnerExceptionMessages)
        {
            var res = ex.Message;
            var innerEx = ex.InnerException;
            var innerExNo = 1;
            //if (innerEx != null)
            //    res += "\r\n";

            while (innerEx != null)
            {
                res += "\r\n" + innerExNo++.ToString() + ". " + innerEx.Message;
                innerEx = innerEx.InnerException;

                if ((maxInnerExceptionMessages < 0) || (innerExNo >= maxInnerExceptionMessages))
                    return res;
            }

            return res;
        }

        public static string GetAllMessages(this Exception ex)
        {
            return ex.GetAllMessages(-1);
        }

        public static string GetExtendedMessage(this Exception ex, string header, string footer, bool withStackTrace)
        {
            string res = header + "";
            if (!string.IsNullOrEmpty(res))
                res += "\r\n";

            res += ex.GetAllMessages();

            if (withStackTrace)
            {
                res += "\r\n";
                var targetInfo = ex.GetTargetInfo();
                if (targetInfo != null)
                    res += "\r\nError source: " + targetInfo;

                var traceInfos = ex.GetTraceInfos();
                if (traceInfos != null)
                {
                    res += "\r\nTrace Info: ";
                    foreach (var trInfo in traceInfos)
                    {
                        res += "\r\n   - " + trInfo;
                    }
                    res += "\r\n";
                }

                if (withStackTrace)
                    res += "\r\n\r\nStack Trace: \r\n" + GetStackTraces(ex) + "\r\n";
            }

            if (!string.IsNullOrEmpty(footer))
                res += "\r\n" + footer;
            return res;
        }

        private static string GetTargetInfo(this Exception ex)
        {
            //return ex.TargetSite.DeclaringType.Name + "." + ex.TargetSite.Name + " " + ex.TargetSite.MemberType.ToString();
            return null;
        }

        public static void AddTraceInfo(this Exception ex, string method)
        {
            if (!ex.Data.Contains(TraceInfoKey))
                ex.Data[TraceInfoKey] = new List<string>();
            var traceInfos = (List<string>)ex.Data[TraceInfoKey];
            traceInfos.Add(method);
        }

        public static List<string> GetTraceInfos(this Exception ex)
        {
            if (!ex.Data.Contains(TraceInfoKey))
                return null;
            return ex.Data[TraceInfoKey] as List<string>;
        }

        public static string GetStackTraces(this Exception exception)
        {
            string fullStackTrace = exception.StackTrace;

            Exception innerException = exception.InnerException;
            while (innerException != null)
            {
                fullStackTrace += "\n\nCaused by: " + exception.Message + "\n\n" + exception.StackTrace;
                innerException = innerException.InnerException;
            }
            return fullStackTrace;
        }


    }

}
