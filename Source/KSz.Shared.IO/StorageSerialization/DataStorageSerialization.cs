using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO
{
    // Abstraction classes for implementing different sequential data providers:
    // BlhPoint, EgbIdd, ...

    // And every data mapper (eg. BlhPoint) can load data from different 
    // storage formats (TXT, CSV, GSI, SHP, ...)


    public class DataStorageSerialization : Presenter
    {
        public DataStorageSerialization(DataStorageInfo storageInfo)
        {
            this.StorageInfo = storageInfo;
        }

        public DataStorageInfo StorageInfo { get; private set; }

        private string mSampleDataText;
        public string SampleDataText
        {
            get { return mSampleDataText; }
            set
            {
                if (NotifyPropertyChanged(ref mSampleDataText, value, () => this.SampleDataText))
                {
                    NotifyPropertyChanged(() => this.HasSampleData);
                }
            }
        }
        public bool HasSampleData { get { return !string.IsNullOrEmpty(SampleDataText); } }    
    }


    /// <summary>
    /// Write data to storage
    /// </summary>
    public class DataStorageSerializer : DataStorageSerialization
    {

        public DataStorageSerializer(DataStorageInfo storageInfo) : base(storageInfo)
        {

        }
        //public void Serialize(XmlWriter xmlWriter, object o);

    }


    /// <summary>
    /// Read data from storage
    /// </summary>
    public class DataStorageDeserialzier: DataStorageSerialization
    {

        public DataStorageDeserialzier(DataStorageInfo storageInfo) : base(storageInfo)
        {

        }
        //public object Deserialize(TextReader textReader);
    }

    public class SequentialDataReader<T> : DataStorageSerialization
    {
        public SequentialDataReader(DataStorageInfo storageInfo) : base(storageInfo)
        {
        }

        public virtual List<T> ReadAll()
        {
            return new List<T>();
        }
    }


    /// <summary>
    /// There are always two stages of importing some data: first You have to read data from source 
    /// and then you have to write this data to your application. First stage is managed through SequentialDataReader(T).
    /// Second stage could be managed with help of SequentialDataLoader.
    /// </summary>
    public class SequentialDataImporter<T> : DataStorageSerialization
    {

        public SequentialDataImporter(DataStorageInfo storageInfo) : base(storageInfo)
        {

        }

        public virtual void Import(List<T> items)
        {

        }
    }


    public class SequentialDataWriter<T> : DataStorageSerialization
    {

        public SequentialDataWriter(DataStorageInfo storageInfo) : base(storageInfo)
        {

        }
    }
}
