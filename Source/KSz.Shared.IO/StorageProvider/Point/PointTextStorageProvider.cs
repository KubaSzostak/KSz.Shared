using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace System.IO
{


    public class PointTextStorageProvider<T> : PointStorageProvider<T> where T : PointBase
    {
        private bool _useId = true;
        public bool UseId
        {
            get { return _useId; }
            set
            {
                if (OnPropertyChanged(ref _useId, value, nameof(UseId)))
                {
                    OnSettingsChanged();
                }
            }
        }


        private bool _changeCoordinatesOrder = false;
        public bool ChangeCoordinatesOrder
        {
            get { return _changeCoordinatesOrder; }
            set
            {
                if (OnPropertyChanged(ref _changeCoordinatesOrder, value, nameof(ChangeCoordinatesOrder)))
                {
                    OnSettingsChanged();
                }
            }
        }

        private bool _haveThirdCoordinate = true;
        public bool HasThirdCoordinate
        {
            get { return _haveThirdCoordinate; }
            protected set
            {
                if (OnPropertyChanged(ref _haveThirdCoordinate, value, nameof(HasThirdCoordinate)))
                {
                    if (!_haveThirdCoordinate)
                        UseThirdCoordinate = false;
                }
            }
        }
        

        public bool _useThirdCoordinate = true;
        public bool UseThirdCoordinate
        {
            get { return _useThirdCoordinate; }
            set
            {
                if (OnPropertyChanged(ref _useThirdCoordinate, value, nameof(UseThirdCoordinate)))
                {
                    OnPropertyChanged(nameof(IgnoreThirdCoordinate));
                    OnSettingsChanged();
                }
            }
        }

        public bool IgnoreThirdCoordinate
        {
            get { return !UseThirdCoordinate; }
            set { UseThirdCoordinate = !value; }
        }

        private bool _haveCode = true;
        public bool HasCode
        {
            get { return _haveCode; }
            set
            {
                if (OnPropertyChanged(ref _haveCode, value, nameof(HasCode)))
                {
                    if (!_haveCode)
                        UseCode = false;
                }
            }
        } 

        private bool _useCode = false;
        public bool UseCode
        {
            get { return _useCode; }
            set
            {
                if (OnPropertyChanged(ref _useCode, value, nameof(this.UseCode)))
                {
                    OnPropertyChanged(nameof(IgnoreCode));
                    OnSettingsChanged();
                }
            }
        }

        public bool IgnoreCode
        {
            get { return !UseCode; }
            set { UseCode = !value; }
        }

        private bool _useAdditionalFields = false;
        public bool UseAdditionalFields
        {
            get { return _useAdditionalFields; }
            set
            {
                if (OnPropertyChanged(ref _useAdditionalFields, value, nameof(this.UseAdditionalFields)))
                {
                    OnSettingsChanged();
                }
            }
        }

        public readonly List<string> AdditionalFieldNames = new List<string>();

        protected string GetAdditionalFieldName(int index)
        {
            if ((index >= 0) && (index < this.AdditionalFieldNames.Count))
                return this.AdditionalFieldNames[index];
            else
                return "F_" + index.ToString("00");
        }



        public IList<PointField> Fields { get; private set; } = new List<PointField>() { PointField.Id, PointField.Coord1, PointField.Coord2 };

        /// <summary>
        /// Point fields (Id, Coord1, Coord2, ...) and AdditionalFields 
        /// </summary>
        public IList<string> FieldNames { get; private set; }        
        

        protected override void OnSettingsChanged()
        {
            base.OnSettingsChanged();

            this.Fields = new List<PointField>();

            if (this.UseId)
                Fields.Add(PointField.Id);

            if (this.ChangeCoordinatesOrder)
                Fields.AddItems(PointField.Coord2, PointField.Coord1);
            else
                Fields.AddItems(PointField.Coord1, PointField.Coord2);

            if (this.HasThirdCoordinate && this.UseThirdCoordinate)
                Fields.Add(PointField.Coord3);

            if (this.HasCode && this.UseCode)
                Fields.Add(PointField.Code);

            OnPropertyChanged(nameof(Fields));

            
            var fNames = new List<string>();
            fNames.AddRange(SampleData.FieldNames(Fields.ToArray()));
            if (this.UseAdditionalFields)
                fNames.AddRange(AdditionalFieldNames);
            this.FieldNames = fNames;
            this.OnPropertyChanged(nameof(FieldNames));

            
            SampleDataFormat = FieldNames.Join(", "); // FieldNames already contain AdditionalFieldNames, see lines above
        }

        protected override void OnSampleDataChanged()
        {
            base.OnSampleDataChanged();
            SampleDataText = SampleData.ToString();
        }
    }



    public class PointTextStorageReader<T> : PointTextStorageProvider<T> where T : PointBase
    {

        private TextLine _sampleLine = null;
        public TextLine SampleLine
        {
            get { return _sampleLine; }
            set
            {
                if (OnPropertyChanged(ref _sampleLine, value, nameof(SampleLine)))
                {
                    this.SampleDataText = _sampleLine?.SourceText;
                    this.SampleData = this.TextLineToPoint(_sampleLine);
                }
            }
        } 


        protected virtual T TextLineToPoint(TextLine ln)
        {
            var pt = Activator.CreateInstance<T>();

            for (int i = 0; i < this.Fields.Count; i++)
            {
                var ptField = this.Fields[i];
                var lnField = ln.Fields[i];
                pt.SetFieldValue(ptField, lnField);
            }

            for (int i = this.Fields.Count - 1; i < ln.Fields.Length; i++)
            {
                pt.AdditionalFields.Add(ln.Fields[i]);
            }
            return pt;
        }

        protected override void OnSettingsChanged()
        {
            base.OnSettingsChanged();
            if (this.SampleLine != null)
            {
                if (this.SampleLine.Fields.Length < this.Fields.Count)
                {
                    this.SetErrorFmt("Invalid points source format. There must be at least {0} fields", this.Fields.Count);
                    return;
                }
                try
                {
                    this.SampleData = TextLineToPoint(this.SampleLine);

                    var fieldCount = SampleLine.Fields.Length;
                    if (UseId)
                    {
                        fieldCount = fieldCount - 1;
                    }

                    this.HasThirdCoordinate = this.HasThirdCoordinate && (fieldCount > 2);// (Id), X, Y, Z
                    this.HasCode = this.HasCode && (fieldCount > 3); // (Id), X, Y, Z, Code
                    //this.SampleDataText = SampleLine.SourceText; // Set SampleDataText from SampleData, not from SampleLine
                }
                catch (Exception ex)
                {
                    this.SetError(ex.Message);
                }
            }




        }

        public void ReadPoints(TextLinesReader storage, Action<T> onRead)
        {
            foreach (var ln in storage)
            {
                try
                {
                    var pt = TextLineToPoint(ln);
                    onRead(pt);
                }
                catch (Exception ex)
                {
                    if (ex is TextLineException)
                        throw;
                    else
                        throw new TextLineException(ln, "Invalid point data format", ex);
                }
            }
        }

        public void ReadPoints(TextLinesReader storage, PointRepository<T> repository)
        {
            this.ReadPoints(storage, pt => repository.Add(pt));
        }

    }



    public class PointTextStorageWriter<T> : PointTextStorageProvider<T> where T : PointBase
    {
        private bool _writeHeader = false;
        public bool WriteHeader
        {
            get { return _writeHeader; }
            set { OnPropertyChanged(ref _writeHeader, value, nameof(this.WriteHeader)); }
        }

        public readonly IList<string> Header = new List<string>();
        public void AddHeader(string header)
        {
            this.Header.Add(header);
        }

        protected void SetTextLineFieldWidht(TextLinesWriter storage, IEnumerable<T> points)
        {
            if (storage.FieldType == TextFieldType.AutoWidht)
            {
                foreach (var pt in points)
                {
                    // TODO: Consider large datasets: eg. only first and last points
                    var fields = this.GetPointFields(pt);
                    storage.SetFieldsWidth(fields);
                }
            }            
        }

        public void WritePoints(TextLinesWriter storage, IEnumerable<T> points)
        {
            SetTextLineFieldWidht(storage, points);
            if (this.WriteHeader)
                storage.WriteHeader(this.FieldNames, this.Header);

            // WriteTextLinePoints
            foreach (var pt in points)
            {
                var fields = this.GetPointFields(pt);
                storage.WriteFields(fields);
            }
        }

        public virtual IList<string> GetPointFields(T point)
        {
            var res = new List<string>();

            res.AddRange(point.GetFieldValues(this.Fields));
            if (this.UseAdditionalFields)
                res.AddRange(point.AdditionalFields);

            return res;
        }

        protected override void OnSettingsChanged()
        {
            base.OnSettingsChanged();
            SampleDataText = this.GetPointFields(this.SampleData).Join("  ");
        }

        private string mCoordinatesPrecision = XYCoordinate.DefaultPrecision;
        public string CoordinatesPrecision
        {
            get { return mCoordinatesPrecision; }
            set { OnPropertyChanged(ref mCoordinatesPrecision, value, nameof(CoordinatesPrecision)); }
        }

        private string mThirdCoordinatePrecision = HCoordinate.DefaultPrecision;
        public string ThirdCoordinatePrecision
        {
            get { return mThirdCoordinatePrecision; }
            set { OnPropertyChanged(ref mThirdCoordinatePrecision, value, nameof(ThirdCoordinatePrecision)); }
        }

        public string[] PrecisionList { get; private set; } = new string[] { "0", "0.0", "0.00", "0.000", "0.0000" };

    }


}
