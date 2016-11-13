using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{



    [Obsolete()]
    public class CSVTable : CSVTable<string, string>
    {
    }


    [Obsolete()]
    public class CSVTable<TCol, TItem>
    {
        #region *** Row Class ****
        public class Row
        {
            private List<TCol> mColumns;
            private Dictionary<TCol, TItem> mItems = new Dictionary<TCol, TItem>();

            internal Row(List<TCol> columns)
            {
                mColumns = columns;
            }

            public TItem this[TCol col]
            {
                get
                {
                    if (mItems.ContainsKey(col))
                        return mItems[col];
                    if (mColumns.Contains(col))
                        return default(TItem);
                    throw new KeyNotFoundException(string.Format(SysUtils.Strings.ColNotFound, col));
                }
                set
                {
                    if (!mColumns.Contains(col))
                        mColumns.Add(col);
                    mItems[col] = value;
                }
            }
        }
        #endregion

        private List<TCol> mColumns = new List<TCol>();
        public IList<TCol> Columns
        {
            get { return mColumns; }
        }

        public string ColumnSeparator = ";";
        public readonly string Quote = @"""";

        private List<Row> mRows = new List<Row>();
        public IList<Row> Rows
        {
            get { return mRows; }
        }

        public Row AddRow()
        {
            var res = new Row(mColumns);
            mRows.Add(res);
            return res;
        }

        public void SaveToStream(Stream stream)
        {
            var lines = ToStringList();
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                foreach (var ln in lines)
                    writer.WriteLine(ln);
            }
        }

        public IList<string> ToStringList()
        {
            List<string> sl = new List<string>();

            var header = Columns.Select(c => c.ToString()).Join(ColumnSeparator);
            sl.Add(header);

            foreach (var row in Rows)
            {
                List<string> rowItems = new List<string>();
                foreach (var col in Columns)
                {
                    var item = row[col].ToText() ?? "";
                    item = item.Replace(Quote, Quote + Quote); // " -> ""
                    if (item.Contains(ColumnSeparator))
                        item = Quote + item + Quote;
                    rowItems.Add(item);
                }
                sl.Add(rowItems.Join(ColumnSeparator));
            }

            return sl;
        }

    }
}
