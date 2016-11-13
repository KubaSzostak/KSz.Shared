using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Abstraction class for implementing different sequential data repositiories:
    /// BlhPoint, EgbIdd, ... 
    /// 
    /// Every data repository (eg. BlhPoint) can load data from different storage formats (TXT, CSV, GSI, SHP, ...) 
    /// and then store them back to many different storage formats (SPH, TXT, ...)
    /// </summary>
    public class StorageRepository<T> : ObservableObject, IEnumerable<T>, ICollection<T>
    {
        // https://msdn.microsoft.com/en-us/library/ff649690.aspx    


        private readonly List<T> items = new List<T>();
        
        public int Count
        {
            get { return this.items.Count; }
        }

        public virtual void Add(T item)
        {
            this.items.Add(item);

            if (!this.firstIsSet)
            {
                this.First = item;
                this.firstIsSet = true;
            }
            this.Last = item;
            this.Mid = items[(items.Count - 1) / 2];
        }

        public virtual void Clear()
        {
            this.items.Clear();

            this.firstIsSet = false;
            this.First = default(T);
            this.Last = default(T);
            this.Mid = default(T);
        }

        public bool Contains(T item)
        {
            return this.items.Contains(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        private bool firstIsSet = false;
        public T First { get; private set; }
        public T Last { get; private set; }
        public T Mid { get; private set; }
    }




}
