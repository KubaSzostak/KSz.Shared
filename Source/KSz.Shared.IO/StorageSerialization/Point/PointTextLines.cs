using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO
{


    public class TextLinePointReader<T> : DataStoragePointReader<T> where T : PointBase
    {

        public TextLine SampleLine { get; private set; }
        public TextLines Lines { get; private set; }

        public TextLinePointReader(TextLines lines, DataStorageInfo storageInfo, bool useId) : base(storageInfo)
        {
            this.Lines = lines;

            this.SampleLine = this.Lines.GetSampleLine();
            this.SampleDataText = this.SampleLine.SourceText;
            InitSettings(this.SampleLine, useId);

            this.Settings.FormatChangded += (s, e) => { OnFormatChanged(); };
            OnFormatChanged();

        }

        protected virtual void OnFormatChanged()
        {
            if (this.SampleLine.Fields.Length < Settings.Fields.Length)
            {
                this.SetErrorFmt("Invalid points source format. There must be at least {0} fields", Settings.Fields.Length);
            }
            else
            {
                try
                {
                    var pt = TextLineToPoint(this.SampleLine);
                }
                catch (Exception ex)
                {
                    this.SetError(ex.Message);
                }
            }
        }

        protected override List<T> ReadFromStorage()
        {
            var res = new List<T>();
            foreach (var ln in this.Lines)
            {
                try
                {
                    var pt = TextLineToPoint(ln);
                    res.Add(pt);
                }
                catch (Exception ex)
                {
                    if (ex is TextLineException)
                        throw;
                    else
                        throw new TextLineException(ln, "Invalid point data format", ex);
                }
            }
            return res;
        }

        private void InitSettings(TextLine sampleLine, bool useId)
        {

            this.Settings.UseThirdCoordinate = false;
            this.Settings.UseCode = false;
            this.Settings.UseId = useId;


            var fieldCount = sampleLine.Fields.Length;
            if (useId)
            {
                fieldCount = fieldCount - 1;
            }

            if (fieldCount > 2) // (Id), X, Y, Z
            {
                this.Settings.UseThirdCoordinate = true;
            }

            if (fieldCount > 3) // (Id), X, Y, Z, Code
            {
                this.Settings.UseCode = true;
            }
        }

        private T TextLineToPoint(TextLine ln)
        {
            var pt = Activator.CreateInstance<T>();

            for (int i = 0; i < this.Settings.Fields.Length; i++)
            {
                var ptField = this.Settings.Fields[i];
                var lnField = ln.Fields[i];
                pt.SetField(ptField, lnField);
            }
            for (int i = this.Settings.Fields.Length - 1; i < ln.Fields.Length; i++)
            {
                pt.AdditionalFields.Add(ln.Fields[i]);
            }
            return pt;
        }
    }



    public class TextLinePointWriter<T> : DataStoragePointWriter<T> where T : PointBase
    {

        public TextLinePointWriter(TextLineWriter writer) : base(writer.StorageInfo)
        {
            this.Writer = writer;
        }

        public TextLineWriter Writer { get; private set; }

        public override void WriteToStorage(Stream stream, string path)
        {
            Writer.Init(stream, path);

            if (Writer.FieldType == TextFieldType.AutoWidht)
            {
                foreach (var pt in Points)
                {
                    // TODO: Consider large datasets: eg. only first and last points
                    var fields = this.GetPointFields(pt);
                    Writer.SetFieldsWidth(this.GetPointFields(pt));
                }
            }

            if (this.Settings.WriteHeader)
            {
                if (Writer.FieldType != TextFieldType.Delimited)
                {
                    foreach (var headerLine in this.Settings.Header)
                    {
                        Writer.WriteLine(headerLine);
                    }
                }
                Writer.WriteLine(GetPointFieldNames());
            }

            foreach (var pt in Points)
            {
                var fields = this.GetPointFields(pt);
                Writer.WriteLine(fields);
            }
        }


        protected virtual string[] GetPointFields(T point)
        {
            var res = new List<string>();

            if (this.Settings.UseId)
                res.Add(point.GetField(PointField.Id));

            res.Add(point.GetField(PointField.Coord1));
            res.Add(point.GetField(PointField.Coord2));

            if (this.Settings.UseThirdCoordinate)
                res.Add(point.GetField(PointField.Coord3));

            if (this.Settings.UseCode)
                res.Add(point.GetField(PointField.Code));

            if (this.Settings.UseAdditionalFields)
                res.AddRange(point.AdditionalFields);

            return res.ToArray();
        }

        protected virtual string[] GetPointFieldNames()
        {
            var point = Activator.CreateInstance<T>();
            var res = new List<string>();

            if (this.Settings.UseId)
                res.Add(point.FieldName(PointField.Id));

            res.Add(point.FieldName(PointField.Coord1));
            res.Add(point.FieldName(PointField.Coord2));

            if (this.Settings.UseThirdCoordinate)
                res.Add(point.FieldName(PointField.Coord3));

            if (this.Settings.UseCode)
                res.Add(point.FieldName(PointField.Code));

            if (this.Settings.UseAdditionalFields)
                res.AddRange(this.Settings.AdditionalFieldNames);

            res[0] = "#" + res[0];
            return res.ToArray();
        }

    }




}
