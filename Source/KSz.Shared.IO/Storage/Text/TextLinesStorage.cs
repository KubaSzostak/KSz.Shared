using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{


    public class TextLinesReader : TextStorageReaderBase, IEnumerable<TextLine>
    {


        public TextLinesReader()
        {
            this.Description = "Plain text";
            this.Extensions = new string[] { ".txt" }; 
        }

        /// <summary>
        /// Defines comment tokens. A comment token is a string that indicates this token and all text placed after this token is a comment and should be ignored by the parser.
        /// </summary>
        public string[] CommentTokens = new string[0];  // "#", "//", "'"
        public long MaxLineLength = 1024 * 1024;

        /// <summary>
        /// Indicates whether leading and trailing white space should be trimmed from field values.
        /// </summary>
        public bool TrimWhiteSpace = false;

        public bool SkipEmptyLines = false;

        private int lineNo = 0;

        public TextLine GetSampleLine()
        {
            if (this.Storage == null)
                throw new NullReferenceException(this.GetType().Name + "." + nameof(Stream) + " not initialized");

            if (!this.Storage.BaseStream.CanSeek)
                throw new NotSupportedException(this.GetType().Name + " does not support SampleLine (" + this.Path + ")");
            
            var pos = this.Storage.BaseStream.Position;
            try
            {
                var lines = new TextLines();
                foreach (var ln in this)
                {
                    lines.Add(ln);
                    if (lines.Count > 256) // do not search gigabytes of data
                        break;
                }
                return lines.GetSampleLine();
            }
            finally
            {
                lineNo = 0;
                this.Storage.BaseStream.Position = pos;
            }
        }

        private string GetText(string ln)
        {
            if (CommentTokens != null)
            {
                foreach (var token in CommentTokens)
                {
                    var ti = ln.IndexOf(token);
                    if (ti == 0)
                        return string.Empty;

                    if (ti > 0)
                        ln = ln.Substring(0, ti);
                }
            }

            if (TrimWhiteSpace)
                ln = ln.Trim();

            return ln;
        }

        protected virtual string[] GetFields(TextLine ln)
        {
            return ln.Data.Split();
        }

        private void SetFieldsInternal(TextLine ln, string[] fields)
        {
            ln.Fields = fields;
            if (this.TrimWhiteSpace)
            {
                for (int i = 0; i < ln.Fields.Length; i++)
                {
                    ln.Fields[i] = ln.Fields[i].Trim();
                }
            }
        }

        private TextLine ReadLineInternal(string lnData, int lnNo)
        {
            var ln = new TextLine(lnData, lnNo);
            var fields = GetFields(ln);
            SetFieldsInternal(ln, fields);

            return ln;
        }

        public TextLine ReadLine()
        {
            var ln = Storage.ReadLine();

            if (ln == null)
                return null;

            lineNo++;

            if (ln.Length > MaxLineLength) {
                var textLn = new TextLine(ln, lineNo);
                throw new TextLineException(textLn, "Line too long: " + ln.Substring(0, 64) + "...");
            }

            ln = SysUtils.TrimBom(ln);
            ln = GetText(ln);

            if (SkipEmptyLines && string.IsNullOrEmpty(ln))
                return ReadLine();

            return ReadLineInternal(ln, lineNo);
        }

        public TextLines ReadAll()
        {
            var res = new TextLines();
            ReadAll((ln) => { res.Add(ln); });
            return res;
        }

        public void ReadAll(Action<TextLine> action)
        {
            Storage.BaseStream.Position = 0;
            while (!Storage.EndOfStream)
            {
                var ln = ReadLine();
                if (ln != null)
                    action(ln);
            }
        }


        public void SetCommentTokens(params string[] tokens)
        {
            this.CommentTokens = tokens;
        }

        public void SetDefaultCommentTokens()
        {
            this.SetCommentTokens("#", "//", "'");
        }

        public void InitDelimitedFieldsReader()
        {
            SetDefaultCommentTokens();
            TrimWhiteSpace = true;
            SkipEmptyLines = true;
        }

        IEnumerator<TextLine> IEnumerable<TextLine>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<TextLine>).GetEnumerator();
        }

        private class Enumerator : DisposableObject, IEnumerator<TextLine>
        {
            public Enumerator(TextLinesReader reader)
            {
                this.Reader = reader;
            }

            TextLinesReader Reader;

            public TextLine Current { get; private set; }

            object IEnumerator.Current { get { return this.Current; } }

            public bool MoveNext()
            {
                this.Current = this.Reader.ReadLine();
                return this.Current != null;
            }

            public void Reset()
            {
                throw new NotSupportedException(this.Reader.GetType().Name + " does not support Enumerator reseting");
            }
            
        }

    }




    public class TextLinesWriter : TextStorageWriter
    {

        private Dictionary<int, int> FieldWidth = new Dictionary<int, int>(); //FieldNo, MaxFieldLenght

        private int DefaultFieldLength = 6;
        public string Delimiter = "   ";
        public TextFieldType FieldType = TextFieldType.AutoWidht;
        public int LinesWritten { get; private set; } = 0;

        public TextLinesWriter() : this(TextFieldType.AutoWidht, "   ")
        {
        }

        public TextLinesWriter(TextFieldType fieldType, string delimiter)
        {
            this.FieldType = fieldType;
            this.Delimiter = delimiter;
            this.Extensions = new string[] { ".txt" }; 

            switch (fieldType)
            {
                case TextFieldType.Delimited:
                    Description = SysUtils.Strings.DelimitedFileDescription(delimiter);
                    break;
                case TextFieldType.FixedWidth:
                    Description = SysUtils.Strings.FixedWidthText;
                    break;
                //case TextFieldType.AutoWidht:
                //    break;
                default:
                    this.Description = "Plain text";
                    break;
            }
        }
        

        public void WriteFields(IEnumerable<string> lnFields)
        {
            WriteFields(lnFields.ToArray());
        }

        public void WriteFields(params string[] lnFields)
        {
            // this method with name WriteLines is used as defeault and base.WriteFields(string) is ingored 
            // when putting only one string as argument

            lnFields = UpdateFieldWidth(lnFields);
            Storage.WriteLine(string.Join(Delimiter, lnFields));
            this.LinesWritten++;
        }

        private string[] UpdateFieldWidth(string[] fields)
        {
            var fieldsCopy = new string[fields.Length];
            for (int i = 0; i < fieldsCopy.Length; i++)
            {
                // val + "" -> avoid null values on field.Length and field.PadLeft() calling
                fieldsCopy[i] = (fields[i] + "").Replace(this.Delimiter, "_?_"); //¤ 
            }

            if (this.FieldType == TextFieldType.Delimited)
                return fieldsCopy;

            for (int fieldNo = 0; fieldNo < fieldsCopy.Length; fieldNo++)
            {
                var field = fieldsCopy[fieldNo] ?? ""; 
                if (this.FieldType == TextFieldType.AutoWidht)
                {
                    SetFieldWidth(fieldNo, field.Length);
                }
                var fieldWidth = GetFieldWidth(fieldNo);
                fieldsCopy[fieldNo] = field.PadLeft(fieldWidth);
            }
            return fieldsCopy;
        }

        private int GetFieldWidth(int fieldNo)
        {
            int fieldLen = 0;
            if (!FieldWidth.TryGetValue(fieldNo, out fieldLen))
                fieldLen = this.DefaultFieldLength;

            return fieldLen;
        }

        public void SetFieldWidth(int fieldNo, int fieldWidth)
        {
            if (this.FieldType == TextFieldType.AutoWidht)
            {
                var savedFieldWidth = GetFieldWidth(fieldNo);
                if (savedFieldWidth < fieldWidth)
                {
                    FieldWidth[fieldNo] = fieldWidth + 2; // Add additional 2 places (AutoWidth)
                }
            }
            else
            {
                FieldWidth[fieldNo] = fieldWidth;
            }
        }



        public void SetFieldsWidth(IEnumerable<int> fieldWidths)
        {
            int fieldNo = 0;
            foreach (var fieldWidth in fieldWidths)
            {
                SetFieldWidth(fieldNo, fieldWidth);
                fieldNo++;
            }
        }

        public void SetFieldsWidth(IEnumerable<string> fields)
        {
            int fieldNo = 0;
            foreach (var field in fields)
            {
                SetFieldWidth(fieldNo, (field + "").Length);
                fieldNo++;
            }
        }

        public void SetFieldsWidth(params int[] fieldWidth)
        {
            SetFieldsWidth((IEnumerable<int>)fieldWidth);
        }

        public void SetFieldsWidth(params string[] fields)
        {
            SetFieldsWidth((IEnumerable<string>)fields);
        }

        public void WriteHeader(IEnumerable<string> fieldNames, IEnumerable<string> headerLines)
        {
            if (this.FieldType == TextFieldType.Delimited)
            {
                this.WriteFields(fieldNames);
            }
            else // It could be space delimited text
            {
                foreach (var headerLine in headerLines)
                {
                    this.WriteLine("# " + headerLine);
                }
                this.WriteLine("# " + fieldNames.Join(", "));
                this.WriteLine("");
            }
        }

    }
}
