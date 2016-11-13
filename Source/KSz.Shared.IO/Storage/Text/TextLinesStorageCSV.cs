using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace System.IO
{


    public class DelimitedTextLinesReader : TextLinesReader
    {
        public DelimitedTextLinesReader(params string[] delimiters)
        {
            this.Delimiters = delimiters;
            base.SkipEmptyLines = true;
            base.TrimWhiteSpace = true;


            if ((delimiters == null) || (delimiters.Length < 1))
            {
                // throw new ArgumentException("Delimiters must be specified in  " + this.GetType().Name, nameof(delimiters));
                this.Description = SysUtils.Strings.DelimitedText + " (Auto)";
            }
            else
            {
                this.Description = SysUtils.Strings.DelimitedFileDescription(delimiters);
            }

            // First line of file could be:
            // sep=<delimeter>
            // sep=;
            // sep=,
            // sep=|
        }

        public string[] Delimiters { get; private set; }

        protected override string[] GetFields(TextLine ln)
        {
            //base.GetFields(ln);

            if (Delimiters != null)
            {
                if (ln.Data.Contains('\t'))
                    Delimiters = new string[] { "\t" };
                else if (ln.Data.Contains(';'))
                    Delimiters = new string[] { ";" };
                else if (ln.Data.Contains(','))
                    Delimiters = new string[] { "," };
                else
                    Delimiters = new string[] { " " };
            }

            return ln.Data.Split(false, Delimiters);
        }
    }


    public class CsvLinesReader : DelimitedTextLinesReader
    {
        public CsvLinesReader() : base(",", ";")
        {
            this.Extensions = new string[] { ".csv" }; 
        }
    }

    public class TabLinesReader : DelimitedTextLinesReader
    {
        public TabLinesReader() : base("\t")
        {
            this.Extensions = new string[] { ".txt", ".tab" }; 
        }
    }






    public class CsvLinesWriter : TextLinesWriter
    {
        public CsvLinesWriter() : base(TextFieldType.Delimited, ";")
        {
            this.Extensions = new string[] { ".csv", ".txt" }; 
        }
    }

    public class TabLinesWriter : TextLinesWriter
    {
        public TabLinesWriter() : base(TextFieldType.Delimited, "\t")
        {
            this.Extensions = new string[] { ".txt", ".tab" }; 
        }
    }

    public static class FileDialogEx
    {

        /// <summary>
        /// Adds CSV, TAB and AUTO DelimitedTextLinesReaders
        /// </summary>
        public static TextLinesReader ShowTextLinesReadersDialog(this IFileDialogService dlg, string filePath)
        {
            dlg.Reset();
            dlg.AddStorage(new TextLinesReader());
            dlg.AddStorage(new CsvLinesReader());
            dlg.AddStorage(new TabLinesReader());
            dlg.AddStorage(new DelimitedTextLinesReader());
            dlg.FilePath = filePath;

            var storage = dlg.ShowDialog();
            if (storage == null)
                return null;

            // Check type during casting
            return (TextLinesReader)storage;
        }

        /// <summary>
        /// Adds CSV, TAB and AUTO DelimitedTextLinesWriters
        /// </summary>
        public static TextLinesWriter ShowTextLinesWritersDialog(this IFileDialogService dlg, string filePath)
        {
            dlg.Reset();
            dlg.AddStorage(new TextLinesWriter());
            dlg.AddStorage(new CsvLinesWriter());
            dlg.AddStorage(new TabLinesWriter());
            dlg.FilePath = filePath;

            var storage = dlg.ShowDialog();
            if (storage == null)
                return null;

            // Check type during casting
            return (TextLinesWriter)storage;
        }
    }
}
