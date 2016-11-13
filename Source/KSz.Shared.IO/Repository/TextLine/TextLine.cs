using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System.IO
{


    public class TextLine : IEnumerable<string>
    {
        public TextLine(string data, int lineNo)
        {
            Source = data;
            SourceText = GetHumanReadableText(Source);
            Data = new StringBuilder(data);
            No = lineNo;
            Fields = new string[0];
            Tags = new Dictionary<object, object>();
        }


        /// <summary>
        /// This line source data readed directly from the stream (whithout any modification)
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Human readable source data (eg. tabulators are replaced with '→')
        /// </summary>
        public string SourceText { get; private set; }

        /// <summary>
        /// Text line data modified by during data reading and parsing (eg. comments removed)
        /// </summary>
        public StringBuilder Data { get; private set; }

        /// <summary>
        /// Human readable text line data modified by during data reading and parsing (eg. comments removed and tabulators are replaced with '→')
        /// </summary>
        public string DataText { get { return GetHumanReadableText(Data.ToString()); } }

        /// <summary>
        /// Line number
        /// </summary>
        public int No { get; private set; }

        /// <summary>
        /// Fields contained within this line. This property should be set by reading/parsing engine.
        /// </summary>
        public string[] Fields { get; set; }

        public string GetField(int index)
        {
            if (index < 0)
                throw new TextLineException(this, "Invalid line field index: " + index.ToString());
            if (index > this.Fields.Length - 1)
                throw new TextLineException(this, string.Format("Line does not conains {0} fields.", index + 1));

            return Fields[index];
        }


        private string GetHumanReadableText(string s)
        {
            return s.Replace('\t', '→');
        }


        public Dictionary<object, object> Tags { get; set; }
        public Exception Error { get; set; }

        public override string ToString()
        {
            return Data.ToString();
        }

        public IEnumerator<string> GetEnumerator()
        {
            if (Fields != null)
                return ((IEnumerable<string>)Fields).GetEnumerator();
            else
                return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (Fields != null)
                return Fields.GetEnumerator();
            else
                return null;
        }
    }
}
