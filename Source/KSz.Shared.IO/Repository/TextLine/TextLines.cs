using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{
    public enum TextFieldType
    {
        Delimited,
        FixedWidth,
        AutoWidht
    }

    public class TextLineWarning : Exception
    {
        public TextLineWarning(TextLine ln, string message) : base(message) { this.Line = ln; }
        public TextLineWarning(TextLine ln, string message, Exception inner) : base(message, inner) { this.Line = ln; }

        public TextLine Line { get; private set; }

        public override string Message
        {
            get
            {
                var res = new StringBuilder();
                res.AppendLine(base.Message);
                res.AppendFormatLine("   Line No:      {0}", Line.No);
                res.AppendFormatLine("   Line Data:    {0}", Line.SourceText);
                if (Line.Data.ToString() != Line.Source)
                {
                    res.AppendFormatLine("   Current Data: {0}", Line.DataText);
                }
                return res.ToString();
            }
        }
    }

    public class TextLineException : TextLineWarning
    {
        public TextLineException(TextLine ln, string message) : base(ln, message) { }
        public TextLineException(TextLine ln, string message, Exception inner) : base(ln, message, inner) { }
    }


    public class TextLines : List<TextLine>, IEnumerable<string>
    {

        public TextLines()
        {
        }


        public TextLines ExctractToLineStartsWith(int maxExtractedLinesCount, params string[] s)
        {
            var res = new TextLines();
            int extrCount = 0;

            while ((this.Count > 0) && !this[0].ToString().StartsWith(s))
            {
                res.Add(this.ExtractFirst());
                if (++extrCount >= maxExtractedLinesCount)
                    throw new TextLineException(this[0], string.Format("Data not found ({0})", maxExtractedLinesCount));
            }
            return res;
        }

        public TextLines ExtractToLineStartsWith(params string[] s)
        {
            return ExctractToLineStartsWith(int.MaxValue, s);
        }

        public TextLines ExtractAfterLastLineStartsWith(params string[] s)
        {
            TextLines res = new TextLines();
            int lasti = IndexOfLastLineStartsWith(s);
            if (lasti < 0)
                return res;

            for (int i = lasti + 1; i < this.Count; i++)
            {
                res.Add(this.ExtractAt(i));
            }
            return res;
        }

        public TextLine FindLineStartsWith(params string[] s)
        {
            int i = IndexOfLineStartsWith(s);
            if (i < 0)
                return null;
            return this[i];
        }

        private int IndexOfLineStartsWith(params string[] s)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].ToString().StartsWith(s))
                    return i;
            }
            return -1;
        }

        private int IndexOfLastLineStartsWith(params string[] s)
        {
            for (int i = this.Count - 1; i >= 0; i--)
            {
                if (this[i].ToString().StartsWith(s))
                    return i;
            }
            return -1;
        }

        private int IndexOfFirstLineStartsWith(params string[] s)
        {
            return IndexOfFirstLine(
                (ln) =>
                {
                    return ln.ToString().StartsWith(s);
                }
           );
        }

        private int IndexOfFirstLine(LinePredicate predicate)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (predicate(this[i]))
                    return i;
            }
            return -1;
        }

        public List<TextLines> ExtractSectionsFromLineStart(string lineStart)
        {
            LinePredicate startSection = (ln) =>
            {
                return ln.ToString().StartsWith(lineStart);
            };
            return ExtractSections(startSection);
        }

        public delegate bool LinePredicate(TextLine ln);

        public List<TextLines> ExtractSections(LinePredicate startSectionFromThisLine)
        {
            var res = new List<TextLines>();
            var s = new TextLines();

            int firstLine = IndexOfFirstLine(startSectionFromThisLine);

            while (firstLine <= this.Count)
            {
                var ln = this.ExtractAt(firstLine);
                if (startSectionFromThisLine(ln))
                {
                    if (s.Count > 0)
                        res.Add(s);
                    s = new TextLines();
                }
                s.Add(ln);
            }
            if (s.Count > 0)
                res.Add(s);
            return res;
        }

        public List<TextLines> ExtractSections(LinePredicate startSectionFromThisLine, LinePredicate endSectionAtThisLine)
        {
            var res = new List<TextLines>();
            TextLines s = null;
            TextLine ln;

            for (int i = this.Count - 1; i >= 0; i--)
            {
                if (endSectionAtThisLine(this[i]))
                {
                    s = new TextLines();
                }

                if (s != null)
                {
                    ln = this.ExtractAt(i);
                    s.Insert(0, ln);

                    if (startSectionFromThisLine(ln))
                    {
                        res.Add(s);
                        s = null;
                    }
                }
            }
            return res;
        }

        public List<TextLines> ExtractSectionsBetween(string startSectionFromLineStartsWith, string endSectionAtLineStartsWith)
        {
            LinePredicate startPredicate = (ln) =>
            {
                return ln.ToString().StartsWith(startSectionFromLineStartsWith);
            };
            LinePredicate endPredicate = (ln) =>
            {
                return ln.ToString().StartsWith(endSectionAtLineStartsWith);
            };

            return ExtractSections(startPredicate, endPredicate);
        }



        public void RemoveLinesThatStartsWith(params string[] s)
        {
            for (int i = this.Count - 1; i >= 0; i--)
            {
                if (this[i].ToString().StartsWith(s))
                    this.RemoveAt(i);
            }
        }

        public void ExtractEmptyLines()
        {
            this.RemoveEmptyValues();
        }

        public void TrimAll()
        {
            foreach (var ln in this)
                ln.Data.Trim();
        }

        public void ReplaceAll(string oldValue, string newValue)
        {
            foreach (var ln in this)
                ln.Data.Replace(oldValue, newValue);
        }

        public void TrimEndAll(string s)
        {
            foreach (var ln in this)
                ln.Data.TrimEnd(s);
        }

        public void TrimAfterAll(string s)
        {
            foreach (var ln in this)
                ln.Data.TrimAfter(s);
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return new StringEnumerator(this);
        }

        private class StringEnumerator : DisposableObject, IEnumerator<string>
        {
            private int _index = -1;
            private TextLines _lines;
            private string _line = null;

            public StringEnumerator(TextLines lines)
            {
                _lines = lines;
            }

            public string Current { get { return _line; } }

            object IEnumerator.Current { get { return _line; } }

            protected override void OnDisposing()
            {
                base.OnDisposing();
            }

            public bool MoveNext()
            {
                _index++;
                if (_index < _lines.Count)
                {
                    _line = _lines[_index].Data.ToString();
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
    }

    public static class TextLinesEx
    {

        /// <summary>
        /// Line with minimum fields
        /// </summary>
        /// <returns></returns>
        public static TextLine GetSampleLine(this IEnumerable<TextLine> lines)
        {
            var res = lines.FirstOrDefault();
            if (res == null)
                return res;

            foreach (var ln in lines)
            {
                if (ln.Fields.Length < res.Fields.Length)
                    res = ln;
            }
            return res;
        }

    }
}
