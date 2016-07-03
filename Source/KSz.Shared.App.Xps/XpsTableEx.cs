using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace System.Windows.Documents
{

    //
    // The diagram below shows the element schema in FlowDocument.
    // http://developer.intersoftsolutions.com/download/attachments/21209814/flowdoc_elementschema%5B1%5D.png
    // https://i-msdn.sec.s-msft.com/dynimg/IC132505.png
    //


    public static class TableEx
    {
        private static Thickness TopThickness = new Thickness(0, 1, 0, 0);
        private static Thickness EmptyThickness = new Thickness(0); 

        public static Table AddTable(this FlowDocument doc)
        {
            var table = new Table();
            table.TextAlignment = TextAlignment.Center;
            table.CellSpacing = 0;
            table.BorderBrush = Brushes.Black;
            table.BorderThickness = new Thickness(0, 0, 0, 1); // Bottom Thickness

            doc.Blocks.Add(table);
            return table;
        }

        public static TableColumn AddColumn(this Table table, int width = -1)
        {
            var cw = GridLength.Auto;
            if (width > 0)
                cw = new GridLength(width, GridUnitType.Pixel);

            var col = new TableColumn { Width = cw };
            table.Columns.Add(col);
            return col;
        }

        public static TextBlock AddColumn(this Table table, string title, int widht = -1)
        {
            var col = AddColumn(table, widht);

            var rowGroup = table.RowGroups.FirstOrDefault() ?? AddRowGroup(table);
            var row = rowGroup.Rows.FirstOrDefault() ?? AddRow(rowGroup);
            row.FontWeight = FontWeights.Bold; 
            var res = row.AddNoWrapCell(title, TextAlignment.Center, TopThickness);

            return res;
        }

        public static TableRow AddColumns(this Table table, params string[] columnTitles)
        {
            foreach (var ct in columnTitles)
            {
                AddColumn(table, ct, -1);
            }
            return table.RowGroups.First().Rows.First();
        }

        public static TableColumnCollection AddColumns(this Table table, params int[] columnWidhts)
        {
            foreach (var cw in columnWidhts)
            {
                AddColumn(table, cw);
            }
            return table.Columns;
        }

        #region *** Row ***

        public static TableRowGroup AddRowGroup(this Table table)
        {
            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);
            return rowGroup;
        }

        public static TableRow AddRow(TableRowGroup rowGroup)
        {
            var row = new TableRow();
            rowGroup.Rows.Add(row);
            return row;
        }

        public static TableRow AddRow(this Table table)
        {
            var rowGroup = table.RowGroups.FirstOrDefault() ?? AddRowGroup(table);
            return AddRow(rowGroup);
        }

        public static TableRow AddRow(this Table table, params object[] cells)
        {
            var row = AddRow(table);         

            foreach (var c in cells)
            {
                if (c is Block) {
                    var rCell = row.AddCell(c as Block);
                    rCell.BorderThickness = TopThickness;
                    rCell.TextAlignment = TextAlignment.Center;
                }
                else {
                    var cStr = "";
                    if (c != null)
                        cStr = c.ToString();
                    row.AddCell(cStr, TextAlignment.Center, TopThickness, TextWrapping.WrapWithOverflow);
                }
            }

            return row;
        }

        #endregion

        #region *** Cell ***
        
        public static TableCell AddCell(this TableRow row, Block block)
        {
            TableCell cell = new TableCell(block);
            cell.BorderBrush = Brushes.Black;
            cell.BorderThickness = EmptyThickness;
            row.Cells.Add(cell);

            return cell;
        }

        public static T AddCell<T>(this TableRow row, Thickness borderThickness) where T : Block
        {
            var block = Activator.CreateInstance<T>();
            var cell = row.AddCell(block);
            cell.BorderThickness = borderThickness;

            return block;
        }

        public static T AddCell<T>(this TableRow row) where T : Block
        {
            return AddCell<T>(row, TopThickness);
        }


        public static TextBlock AddCell(this TableRow row, string text, TextAlignment alignment, Thickness borderThicknees, TextWrapping textWrapping)
        {
            var textBlock = new TextBlock();
            textBlock.Inlines.Add(text + "");
            textBlock.TextWrapping = textWrapping;
            textBlock.TextAlignment = alignment;
            textBlock.TextTrimming = TextTrimming.WordEllipsis;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
            

            var par = AddCell<Paragraph>(row, borderThicknees);
            par.Inlines.Add(textBlock);
            par.TextAlignment = alignment;

            return textBlock;
        }

        public static TextBlock AddCell(this TableRow row, string text)
        {
            return AddCell(row, text, TextAlignment.Center, TopThickness, TextWrapping.WrapWithOverflow);
        }


        public static TextBlock AddNoWrapCell(this TableRow row, string text, TextAlignment alignment, Thickness borderThicknees)
        {
            return AddCell(row, text, alignment, borderThicknees, TextWrapping.NoWrap);
        }

        public static TextBlock AddNoWrapCell(this TableRow row, string text, TextAlignment alignment)
        {
            return AddNoWrapCell(row, text, alignment, TopThickness);
        }

        #endregion
    }

}
