using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System
{


    public interface ILoadInfo
    {
        TimeSpan LoadTime { get; }
        string LoadName { get; }
    }

    public class LoadInfo : ILoadInfo
    {

        private int _startTick = Environment.TickCount;

        public string LoadName { get; internal set; }
        public string LoadDsc { get; private set; }
        public TimeSpan LoadTime { get; private set; }

        public LoadInfo(string name, string dsc)
        {
            this.LoadName = name;
            this.LoadDsc = dsc;
            LoadTime = TimeSpan.Zero;
        }

        private bool _loadEnded = false;
        public void LoadEnded()
        {
            if (!_loadEnded)
            {
                _loadEnded = true;
                this.LoadTime = TimeSpan.FromMilliseconds(Environment.TickCount - _startTick);
            }
        }

        public override string ToString()
        {
            return LoadTime.TotalSeconds.ToString("0.0") + ": " + LoadName;
        }

    }

    public class LoadInfos : ILoadInfo, IEnumerable<ILoadInfo>
    {
        private List<ILoadInfo> infos = new List<ILoadInfo>();
        public string LoadName { get; private set; }

        public LoadInfos(string name)
        {
            LoadTime = TimeSpan.Zero;
            LoadName = name;
        }

        public void Add(ILoadInfo info)
        {
            var li = info as LoadInfo;
            if (li != null)
                li.LoadEnded();

            infos.Add(info);
            LoadTime = LoadTime.Add(info.LoadTime);
            currentLoadInfo = new LoadInfo("LoadInfo", null);
        }

        private LoadInfo currentLoadInfo = new LoadInfo("LoadInfo", null);
        public LoadInfo Add(string name)
        {
            var res = currentLoadInfo;
            res.LoadName = name;
            this.Add(res);
            return res;
        }

        public void AddRange(IEnumerable<ILoadInfo> infos)
        {
            foreach (var info in infos)
            {
                this.Add(info);
            }
        }

        public IEnumerator<ILoadInfo> GetEnumerator()
        {
            return infos.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public TimeSpan LoadTime { get; private set; }
    }

    public static class LoadEx
    {
        public static string LoadInfosText(this IEnumerable<ILoadInfo> infos)
        {
            var res = new StringBuilder();
            BuildInfosText(res, infos, "");
            return res.ToString();
        }

        private static void BuildInfosText(StringBuilder str, IEnumerable<ILoadInfo> infos, string prefix)
        {
            foreach (var info in infos)
            {
                var li = info as LoadInfos;
                if (li != null)
                {
                    //str.AppendLine();
                    str.AppendLine(li.LoadTime.TotalSeconds.ToString("0.0") + ":  " + info.LoadName);
                    BuildInfosText(str, li, prefix + "    ");
                    //str.AppendLine(); 
                }
                else
                    str.AppendLine(prefix + info.LoadTime.TotalSeconds.ToString("0.0") + ":  " + info.LoadName);
            }
        }
    }

}
