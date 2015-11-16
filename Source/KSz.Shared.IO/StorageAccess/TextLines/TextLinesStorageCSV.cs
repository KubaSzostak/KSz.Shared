using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO
{


    public class DelimitedTextLinesReader : TextLineReader
    {
        public DelimitedTextLinesReader(params string[] delimiters)
        {
            if ((delimiters == null) || (delimiters.Length < 1))
                throw new ArgumentException("Delimiters must be specified in  " + this.GetType().Name, "delimiters");
            this.Delimiters = delimiters;
            base.SkipEmptyLines = true;
            base.TrimWhiteSpace = true;
            this.StorageInfo.Format = "Delimited text";
        }

        public readonly string[] Delimiters;

        protected override string[] GetFields(TextLine ln)
        {
            //base.GetFields(ln);
            return ln.Data.Split(false, Delimiters);
        }
    }


    public class CSVLinesReader : DelimitedTextLinesReader
    {
        public CSVLinesReader() : base(",", ";")
        {
            StorageInfo.Format = "CSV - Comma separated values";
            StorageInfo.Extension = ".csv";
        }
    }

    public class TabLinesReader : DelimitedTextLinesReader
    {
        public TabLinesReader() : base("\t")
        {
            StorageInfo.Format = "Tab delimited text";
            StorageInfo.Extension = ".txt";
        }
    }



    public class CSVWriter : TextLineWriter
    {
        public CSVWriter()
        {
            StorageInfo.Format = "CSV - Comma separated values";
            StorageInfo.Extension = ".csv";
            Delimiter = ",";
            FieldType = TextFieldType.Delimited;
        }
    }

    public class TabTextWriter : TextLineWriter
    {
        public TabTextWriter()
        {
            StorageInfo.Format = "Tab delimited text";
            StorageInfo.Extension = ".txt";
            Delimiter = "\t";
            FieldType = TextFieldType.Delimited;
        }
    }
}
