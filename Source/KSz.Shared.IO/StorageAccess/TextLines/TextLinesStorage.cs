using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{


    public class TextLineReader : TextStorageReader
    {

        public TextLineReader()
        {
            this.StorageInfo.Format = "Plain text";
            this.StorageInfo.Extension = ".txt";
        }

        /// <summary>
        /// Defines comment tokens. A comment token is a string that indicates this token and all text placed after this token is a comment and should be ignored by the parser.
        /// </summary>
        public string[] CommentTokens = new string[0];  // "#", "//", "'"

        /// <summary>
        /// Indicates whether leading and trailing white space should be trimmed from field values.
        /// </summary>
        public bool TrimWhiteSpace = true;

        public bool SkipEmptyLines = true;

        private int lineNo = 0;


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
            lineNo++;

            if (ln == null)
                return null;

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

    }




    public class TextLineWriter : TextStorageWriter
    {

        private Dictionary<int, int> FieldWidth = new Dictionary<int, int>(); //FieldNo, MaxFieldLenght

        private int DefaultFieldLength = 6;
        public string Delimiter = "   ";
        public TextFieldType FieldType = TextFieldType.AutoWidht;

        public TextLineWriter()
        {
            this.StorageInfo.Format = "Plain text";
            this.StorageInfo.Extension = ".txt";
        }


        public void WriteLine(string ln)
        {
            Storage.WriteLine(ln);
        }

        public void WriteLine(IEnumerable<string> lnFields)
        {
            WriteLine(lnFields.ToArray());
        }

        public void WriteLine(params string[] lnFields)
        {
            lnFields = UpdateFieldWidth(lnFields);
            Storage.WriteLine(string.Join(Delimiter, lnFields));
        }

        private string[] UpdateFieldWidth(string[] fields)
        {
            if (this.FieldType == TextFieldType.Delimited)
                return fields;

            var fieldsCopy = new string[fields.Length];
            for (int fieldNo = 0; fieldNo < fieldsCopy.Length; fieldNo++)
            {
                var field = fields[fieldNo] ?? ""; // val + "" -> avoid null values on PadLeft() calling
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


        public void SetFieldsWidth(params int[] fieldWidth)
        {
            for (int fieldNo = 0; fieldNo < fieldWidth.Length; fieldNo++)
            {
                SetFieldWidth(fieldNo, fieldWidth[fieldNo]);
            }
        }

        public void SetFieldsWidth(params string[] fields)
        {
            for (int fieldNo = 0; fieldNo < fields.Length; fieldNo++)
            {
                SetFieldWidth(fieldNo, fields[fieldNo].Length);
            }
        }

    }
}
