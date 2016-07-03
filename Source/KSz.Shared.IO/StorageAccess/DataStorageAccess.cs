using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{

    // Abstraction classes for implementing different storage formats:
    // TXT, CSV, GSI, SHP, ...


    public class DataStorageInfo
    {
        public DataStorageInfo(string format, string ext, string path)
        {
            this.Format = format;
            this.Extension = ext;
            this.Path = path;
        }

        /// <summary>
        /// Storage format description, eg. 'Comma delimited values'
        /// </summary>
        public string Format { get; internal set; }

        /// <summary>
        /// Storage file extension eg. '.csv'
        /// </summary>
        public string Extension { get; internal set; }

        private string _path;
        /// <summary>
        /// Path to storage source
        /// </summary>
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                Name = (value + "").StringAfterLast("\\");
            }
        }

        /// <summary>
        /// Short name of storage
        /// </summary>
        public string Name { get; internal set; }
    }


    public class DataStorageAccess : DisposableBase 
    {
        // It could be *.SHP where access to many files is required

        public DataStorageAccess()
        {
            this.StorageInfo = new DataStorageInfo(this.GetType().Name, "*", "");
        }

        public DataStorageInfo StorageInfo { get; protected set; }
    }



    public class StreamStorageAcces : DataStorageAccess
    {

        public virtual void Init(Stream stream, string path)
        {
            if (this.Stream != null)
                throw new Exception(this.GetType().Name + " is already initialized");
            this.Stream = stream;

            this.StorageInfo.Path = path;
        }

        private Stream Stream;

        protected override void OnDisposing()
        {
            if (this.Stream != null)
            {
                this.Stream.Dispose();
                this.Stream = null;
            }
            base.OnDisposing();
        }

        public long Size
        {
            get { return Stream.Length; }
        }
    }



    public class TextStorageWriter : StreamStorageAcces
    {
        public override void Init(Stream stream, string path)
        {
            base.Init(stream, path);
            Storage = new StreamWriter(stream, new UTF8Encoding(false));
        }
        
        // BinaryWriter
        protected TextWriter Storage { get; private set; }

        protected override void OnDisposing()
        {
            if (this.Storage != null)
            {
                this.Storage.Flush();
                this.Storage.Dispose();
                this.Storage = null;
            }
            base.OnDisposing();
        }
    }



    public class TextStorageReader : StreamStorageAcces
    {
        public override void Init(Stream stream, string path)
        {
            base.Init(stream, path);
            Storage = new StreamReader(stream, new UTF8Encoding(false));
            Storage.BaseStream.Position = 0;
        }

        // BinaryReader
        protected StreamReader Storage { get; private set; }

        protected override void OnDisposing()
        {
            if (this.Storage != null)
            {
                this.Storage.Dispose();
                this.Storage = null;
            }
            base.OnDisposing();
        }

        public bool EndOfStream
        {
            get { return Storage.EndOfStream; }
        }

        public long Position
        {
            get { return Storage.BaseStream.Position; }
        }

        public double Percent
        {
            get { return 100.0 * Convert.ToDouble(Position) / Convert.ToDouble(Size); }
        }
    }


    
}
