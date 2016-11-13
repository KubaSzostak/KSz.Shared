using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{

    /// Usually there are two stages of importing some data: 
    /// 1. First you have to read data from Storage into internal Repository 
    /// 2. And then you have to write this data into your application. 
    ///
    /// This requires three Storage/Repository levels:
    /// 1. Raw data storage (TXT, CSV, GSI, MDB, SHP, MySQL, Azure Table, ...)
    /// 2. Internal/Temporary data repository (XyzPoint, User, Organization, Image, ...)
    /// 3. Application data (ESRI.MapPoint, Teigha.Geometry.Point3d, Button, ...)
    ///
    /// eg. TextLinesStorage -> BlhPoint -> ESRI.MapPoint and vice versa


        

    /// <summary>
    /// Abstraction class for implementing different storage formats:
    /// TXT, CSV, GSI, MDB, SHP, MySQL, Azure Table, ...
    /// </summary>
    public class Storage : DisposableObject 
    {
        // It could be *.SHP where access to many files is required
        // It couuld be single CSV file
        // It could be AutoCAD DWG database API

        public Storage()
        {
            this.Description = this.GetType().Name;
            this.Extensions = new string[] { "*" };
            this.Path = "";
        }

        
        /// <summary>
        /// Short name of storage
        /// </summary>
        public string Name { get; protected internal set; }

        /// <summary>
        /// Storage format description, eg. 'Comma delimited values'
        /// </summary>
        public string Description { get; protected internal set; }

        /// <summary>
        /// Storage file extensions eg. '.csv'
        /// </summary>
        public string[] Extensions { get; protected internal set; }

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

    }



    public class StreamStorage : Storage, IStreamStorage
    {

        public virtual void Init(Stream stream, string path)
        {
            if (this.Stream != null)
                throw new Exception(this.GetType().Name + " is already initialized");
            this.Stream = stream;

            this.Path = path;
            //this.Extension = IO.Path.GetExtension(path);
        }

        public Stream Stream { get; private set; }

        protected override void OnDisposing()
        {
            var stm = this.Stream;  // Avoid multithreads conflicts - make a copy
            if (stm != null)
            {
                stm.Dispose();
                this.Stream = null;
            }
            base.OnDisposing();
        }

        public long Size
        {
            get { return Stream.Length; }
        }

        public override string ToString()
        {
            return Description + " (" + Extensions.Join(", ") + ")";
        }
    }


    
}
