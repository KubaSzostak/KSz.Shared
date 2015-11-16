using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{

    public class PointStorageSettings<T> : ObservableObject where T : PointBase
    {
        private bool _useId = true;
        public bool UseId
        {
            get { return _useId; }
            set
            {
                NotifyPropertyChanged(ref _useId, value, () => this.UseId);
                UpdatePointFormat();
            }
        }

        private bool mChangeCoordinatesOrder = false;
        public bool ChangeCoordinatesOrder
        {
            get { return mChangeCoordinatesOrder; }
            set
            {
                NotifyPropertyChanged(ref mChangeCoordinatesOrder, value, () => this.ChangeCoordinatesOrder);
                UpdatePointFormat();
            }
        }

        private bool _useThirdCoordinate = true;
        public bool UseThirdCoordinate
        {
            get { return _useThirdCoordinate; }
            set
            {
                NotifyPropertyChanged(ref _useThirdCoordinate, value, () => this.UseThirdCoordinate);
                UpdatePointFormat();
            }
        }

        private bool _useCode = false;
        public bool UseCode
        {
            get { return _useCode; }
            set
            {
                NotifyPropertyChanged(ref _useCode, value, () => this.UseCode);
                UpdatePointFormat();
            }
        }

        private bool _useAdditionalFields = false;
        public bool UseAdditionalFields
        {
            get { return _useAdditionalFields; }
            set
            {
                NotifyPropertyChanged(ref _useAdditionalFields, value, () => this.UseAdditionalFields);
                UpdatePointFormat();
            }
        }

        public readonly List<string> AdditionalFieldNames = new List<string>();


        public string PointFormat { get; private set; }
        public PointField[] Fields { get; private set; }

        internal static T TempPoint = Activator.CreateInstance<T>();
        protected void UpdatePointFormat()
        {
            var fieldList = new List<PointField>();

            if (this.UseId)
                fieldList.Add(PointField.Id);

            if (this.ChangeCoordinatesOrder)
                fieldList.AddItems(PointField.Coord2, PointField.Coord1);
            else
                fieldList.AddItems(PointField.Coord1, PointField.Coord2);

            if (this.UseThirdCoordinate)
                fieldList.Add(PointField.Coord3);

            if (this.UseCode)
                fieldList.Add(PointField.Code);

            this.Fields = fieldList.ToArray();
            PointFormat = TempPoint.FieldNamesText(this.Fields);
            NotifyPropertyChanged(() => PointFormat);

            if (FormatChangded != null)
                FormatChangded(this, EventArgs.Empty);
        }

        public event EventHandler FormatChangded;
    }

    public class PointReaderSettings<T> : PointStorageSettings<T> where T : PointBase
    {

    }

    public class PointWriterSettings<T> : PointStorageSettings<T> where T : PointBase
    {
        private bool mWriteHeader = false;
        public bool WriteHeader
        {
            get { return mWriteHeader; }
            set { NotifyPropertyChanged(ref mWriteHeader, value, () => this.WriteHeader); }
        }

        public readonly List<string> Header = new List<string>();
        public void AddHeader(string header)
        {
            this.Header.Add("# " + header);
        }

    }


    public abstract class DataStoragePointReader<T> : DataStorageSerialization where T : PointBase
    {
        // Should be valid for:
        //   [X Y Z]
        //   [North East Height]
        //   [Latitude Longitude Elevation]
        
        public DataStoragePointReader(DataStorageInfo storageInfo) : base(storageInfo)
        {
            this.Settings = new PointReaderSettings<T>();

            this.Min = Activator.CreateInstance<T>();
            this.Min.Coord1.Value = double.MaxValue;
            this.Min.Coord2.Value = double.MaxValue;
            this.Min.Coord3.Value = double.MaxValue;

            this.Max = Activator.CreateInstance<T>();
            this.Max.Coord1.Value = double.MinValue;
            this.Max.Coord2.Value = double.MinValue;
            this.Max.Coord3.Value = double.MinValue;


        }


        public PointReaderSettings<T> Settings  { get; private set;  }


        public T Min { get; private set; }
        public T Max { get; private set; }
        


        protected virtual void OnRead(T point)
        {

            this.Max.Coord1.Value = Math.Max(this.Max.Coord1.Value, point.Coord1.Value);
            this.Max.Coord2.Value = Math.Max(this.Max.Coord2.Value, point.Coord2.Value);
            this.Max.Coord3.Value = Math.Max(this.Max.Coord3.Value, point.Coord3.Value);

            this.Min.Coord1.Value = Math.Min(this.Min.Coord1.Value, point.Coord1.Value);
            this.Min.Coord2.Value = Math.Min(this.Min.Coord2.Value, point.Coord2.Value);
            this.Min.Coord3.Value = Math.Min(this.Min.Coord3.Value, point.Coord3.Value);
        }

        protected abstract List<T> ReadFromStorage();

        /// <summary>
        /// There are always two stages of importing some data: first You have to read data from source 
        /// and then You have to write this data to Your application. First stage is managed through PointReader(T).
        /// Second stage could be managed with help of PointLoader.
        /// 
        /// eg. BlhPoint -> ESRI.MapPoint and vice versa
        /// </summary>
        public void LoadAll(Action<T> action)
        {
            var points = this.ReadFromStorage(); // Check all point during reading
            foreach (var pt in points)
            {
                action(pt);
            }
        }

    }



    public abstract class DataStoragePointWriter<T> : DataStorageSerialization where T : PointBase
    {
        // Should be valid for:
        //   [X Y Z]
        //   [North East Height]
        //   [Latitude Longitude Elevation]

        public DataStoragePointWriter(DataStorageInfo storageInfo) : base(storageInfo)
        {
            this.Settings = new PointWriterSettings<T>();
            this.Settings.FormatChangded += (s, e) => { OnFormatChanged(); };
            this.SamplePoint = PointWriterSettings<T>.TempPoint;
        }

        protected virtual void OnFormatChanged()
        {
            SamplePoint = Points.FirstOrDefault() ?? SamplePoint;
            SampleDataText = GetSampleDataText();
        }

        public IList<T> Points { get; set; }

        private T _samplePoint;
        public T SamplePoint
        {
            get { return _samplePoint; }
            set
            {
                if (NotifyPropertyChanged(ref _samplePoint, value, () => this.SamplePoint))
                {
                    this.SampleDataText = GetSampleDataText();
                }                
            }
        }

        public PointWriterSettings<T> Settings { get; private set; }

        protected string GetSampleDataText()
        {
            var res = new List<String>();
            foreach (var fld in this.Settings.Fields)
            {
                var fData = this.SamplePoint.GetField(fld);
                res.Add(fData);
            }
            return res.Join("  ");
        }

        public abstract void WriteToStorage(Stream stream, string path);
    }

    
    

}
