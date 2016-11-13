using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Abstraction class for implementing providers between different storage formats and repositories:
    /// TXT -> XyzPoint,   TXT -> TpsMeasurement,   TXT -> User,        
    /// XyzPoint -> TXT,   XyzPoint -> CSV,         XYZPoint -> SHP,    ...., 
    /// and vice versa
    /// </summary>
    public class StorageProvider<T> : Presenter
    {
        // Settings
        // LoadFromStorage
        // SaveToStorage

        public event EventHandler SettingsChangded;

        protected virtual void OnSettingsChanged()
        {
            SettingsChangded?.Invoke(this, EventArgs.Empty);
        }


        private T _sampleData = Activator.CreateInstance<T>();
        public T SampleData
        {
            get { return _sampleData; }
            set
            {
                if (OnPropertyChanged(ref _sampleData, value, nameof(SampleData)))
                {
                    OnSampleDataChanged();
                    // OnSettingsChanged();

                    // Consider import from text file. Changing UseThirdCoordinate property will 
                    // change SampleData caclucated from SampleDataText. This will couse infinite loop.

                    // Import from text file: SampleData would be updated during every OnSettingsChanged
                    // Export to text file:   SampleData would be set once on initialization
                }
            }
        }


        public event EventHandler SampleDataChangded;

        protected virtual void OnSampleDataChanged()
        {
            SampleDataChangded?.Invoke(this, EventArgs.Empty);
        }

        private string _sampleDataText;
        /// <summary>
        /// Eg. A 11.11 22.22 33.33 Fix
        /// If reading eg. BlhPoints from text file it should show how first line looks like.
        /// If writing eg. BlhPoints in to text file it should show how sample line would look like (including coordinates precision and other format settings)
        /// </summary>
        public string SampleDataText
        {
            get { return _sampleDataText; }
            protected set 
            {
                if (OnPropertyChanged(ref _sampleDataText, value, nameof(SampleDataText)))
                {
                    OnPropertyChanged(nameof(this.HasSampleDataText));

                    // OnSettingsChanged();                    

                    // Import from text file: SampleDataText would be set once on initialization
                    // Export to text file:   SampleDataText would be updated during every OnSettingsChanged
                }
            }
        }

        public bool HasSampleDataText { get { return !string.IsNullOrEmpty(SampleDataText); } }

        private string _sampleDataFormat = "";
        /// <summary>
        /// Eg. Id Y X Z Code
        /// </summary>
        public string SampleDataFormat
        {
            get { return _sampleDataFormat; }
            set { OnPropertyChanged(ref _sampleDataFormat, value, nameof(SampleDataFormat)); }
        }
    }
}
