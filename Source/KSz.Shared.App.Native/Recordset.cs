using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.Data
{
    public delegate void RecordHandler(object sender, Record record);

    [AttributeUsage(AttributeTargets.Property)]
    public class RecordValueAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Property)]
    public class RecordIdAttribute : RecordValueAttribute
    { }
    
    public class RecordException : Exception
    {
        public RecordException() { }
        public RecordException(string message) : base(message) { }
        public RecordException(string message, Exception inner) : base(message, inner) { }
    }



    public class ValidatingObject : EditableObject, IDataErrorInfo
    {

        private Dictionary<string, List<string>> mErrors = new Dictionary<string, List<string>>();

        protected void AddValidationError(string propName, string errorMessage)
        {
            mErrors.AddListItem(propName, errorMessage);
            OnPropertyChanged(() => HasValidationErrors);
        }

        protected void ClearValidationErros(string propName)
        {
            mErrors.Remove(propName);
            OnPropertyChanged(() => HasValidationErrors);
        }

        protected virtual void ValidateDataAnnotations(string propertyName)
        {
            // Not supported in .NET 3.5, valid from 4.0

            //var props = SysUtils.GetProperties(this, false);

            //string error = string.Empty;

            //var value = GetValue(propertyName);
            //var results = new List<ValidationResult>(1);
            //var result = Validator.TryValidateProperty(
            //    value,
            //    new ValidationContext(this, null, null)
            //    {
            //        MemberName = propertyName
            //    },
            //    results);

            //if (!result)
            //{
            //    var validationResult = results.First();
            //    error = validationResult.ErrorMessage;
            //}

            //return error;
        }

        public override void NotifyPropertyChanged(PropertyChangedEventArgs args)
        {
            ValidateDataAnnotations(args.PropertyName);
            base.NotifyPropertyChanged(args);
        }
        
        protected internal bool HasValidationErrors
        {
            get
            {
                return mErrors.Count == 0;
            }
        }


        string IDataErrorInfo.Error
        {
            get
            {
                if (mErrors.Count == 0)
                    return null;
                return mErrors.Values.First().Join(Environment.NewLine);
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (mErrors.ContainsKey(columnName))
                    return mErrors[columnName].Join(Environment.NewLine);
                return null;
            }
        }
    }
    
    public abstract class Record : ValidatingObject
    {
        public bool RecordIsChanged { get; private set; }
        public bool RecordIsNew { get; private set; }
        public bool RecordIsVirtual { get; internal set; }

        internal HashSet<System.Collections.IList> Views = new HashSet<System.Collections.IList>();
        
        public Record()
        {
            RecordIsChanged = false;
            RecordIsNew = true;
        }

        private RecordProvider mRecordProvider;
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public RecordProvider RecordProvider
        {
            get { return mRecordProvider; }
            set
            {
                if (object.Equals(mRecordProvider, value))
                    return; // nothing changed, nothing to do  

                if (value == null || mRecordProvider == null)
                {
                    mRecordProvider = value;
                }
                else
                {
                    throw new RecordException("This record (" + this.GetType().Name + ") is already assigned to another RecordProvider: " + mRecordProvider.ToString());
                }
            }
        }


        public virtual object GetRecordId()
        {
            return RecordProvider.RecordIdProperty.GetValue(this, null);
            //return GetRecordValue(DbProvider.DbIdProperty);
        }

        public override void NotifyPropertyChanged(PropertyChangedEventArgs args)
        {
            base.NotifyPropertyChanged(args);
            RecordIsChanged = true;
        }

        public static DateTime StringToDateTime(string s)
        {
            return DateTime.ParseExact(s, "s", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string DateTimeToString(DateTime d)
        {
            return d.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
        }

        internal protected void SetRecordSaved()
        {
            RecordIsChanged = false;
            RecordIsNew = false;
            RecordIsVirtual = false;
        }

        public void SetRecordChanged()
        {
            if (!RecordIsNew && !RecordIsChanged)
                RecordIsChanged = true;
        }

        internal void SetRecordVirtual()
        {
            this.RecordIsVirtual = true;
            this.RecordIsNew = false;
            this.RecordIsChanged = false;
        }

        public event RecordHandler RecordDeleted;

        protected internal virtual void OnRecordDeleting()
        {
        }

        protected internal virtual void OnRecordDeleted()
        {
            this.SetRecordSaved(); // Do not update it anymore
            foreach (var v in this.Views)
                v.Remove(this);

            if (RecordDeleted != null)
                RecordDeleted(this, this);

            this.RecordProvider = null;// Leave a while recordset, so in calling OnRecordsetDeleging() one can use this.DbProvider property.
        }

        protected internal virtual void OnRecordInitialized()
        {
        }

        public void Delete()
        {
            if (RecordProvider != null)
                RecordProvider.Delete(this);
            else
            {
                OnRecordDeleting();
                OnRecordDeleted();
            }
        }

        public void Save()
        {
            if (RecordProvider != null)
                RecordProvider.Save(this);
            else
                SetRecordSaved();
        }

        public void Load()
        {
            if (RecordProvider != null)
                RecordProvider.Read(this);
            else
                SetRecordSaved();
        }

        private string _fullSearchString = null;
        protected virtual string FullSearchString
        {
            get
            {
                if (string.IsNullOrEmpty(_fullSearchString))
                {
                    var recVals = this.RecordProvider.GetRecordValues(this);
                    var res = new StringBuilder();
                    foreach (var val in recVals.Values)
                    {
                        if (val != null)
                        {
                            res.Append(val.ToString());
                            res.Append(" ");
                        }
                    }
                    _fullSearchString = res.ToString().ToLower();
                }
                return _fullSearchString;
            }
        }

        public bool FullSearchStringContains(string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;

            s = s.ToLower();
            var sItems = s.SplitValues(true, " ");

            foreach (var si in sItems)
            {
                if (!FullSearchString.Contains(si))
                    return false;
            }
            return true;
        }
        


        public override void BeginEdit()
        {
            // If one changes record data in application without saving them all changes will be lost
            // if record load data from database.
            if (!this.RecordIsChanged && !this.RecordIsNew)
                this.Load();

            base.BeginEdit();
            // Nothing to do
        }

        public override void CancelEdit()
        {
            base.CancelEdit();
            // Nothing to do
        }

        public override void EndEdit()
        {
            base.EndEdit();
            this.Save();
        }

    }


    public class SubRecord : ObservableObject
    {
        internal Record OwnerRecord { get; set; }

        public SubRecord()
        {
        }

        public override void NotifyPropertyChanged(PropertyChangedEventArgs args)
        {
            base.NotifyPropertyChanged(args);
            if (this.OwnerRecord != null)
                this.OwnerRecord.SetRecordChanged();
        }
    }


    public class SubRecords<T> : ObservableCollection<T> where T : SubRecord
    {

        public SubRecords(Record rec)
        {
            this.Record = rec;
        }

        private Record Record;

        protected override void OnCollectionChanged(Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            if (this.Record != null)
                this.Record.SetRecordChanged();

            if (e.OldItems != null)
                foreach (var rec in e.OldItems.Cast<SubRecord>())
                {
                    rec.OwnerRecord = null;
                }

            if (e.NewItems != null)
                foreach (var rec in e.NewItems.Cast<SubRecord>())
                {
                    rec.OwnerRecord = this.Record;
                }
        }
    }




    public abstract class RecordProviderBase
    {

        protected internal Type RecordType { get; private set; }
        public IEnumerable<PropertyInfo> RecordProperties { get; private set; }
        public ILookup<string, PropertyInfo> RecordPropertiesLookup { get; private set; }
        protected IEnumerable<string> RecordFieldNames { get; private set; }

        internal RecordProviderBase(Type recType)
        {
            RecordType = recType;

            RecordProperties = RecordType.GetProperties().AsEnumerable();
            //DbProperties = DbProperties.Where(p => p.IsEditableValue() || p.PropertyType.IsSubclassOf(typeof(DbSubRecord)));
            RecordProperties = RecordProperties.Where(p => p.IsDefined(typeof(RecordValueAttribute), true));

            RecordPropertiesLookup = RecordType.GetProperties().ToLookup(p => p.Name);
            RecordFieldNames = RecordProperties.Select(p => p.Name);
        }

        public PropertyInfo GetRecordProperty(string field)
        {
            return RecordPropertiesLookup[field].FirstOrDefault();
        }

        protected void SetRecordValue(object record, string field, object value)
        {
            var prop = GetRecordProperty(field);
            if (prop != null)
            {
                if (DeserializePropertyValue(record, prop, value))
                    return;
            }

            if (record is IDictionary)
            {
                (record as IDictionary)[field] = value;
                return;
            }
            if (record is IList)
            {
                (record as IList).Add(value);
                return;
            }
            if (record is ITagedObject)
            {
                (record as ITagedObject).Tags[field] = value;
                return;
            }

            throw new KeyNotFoundException(string.Format(SysUtils.Strings.FieldNotFound, this.RecordType.Name + ". " + field));
        }

        protected object GetRecordValue(object record, string field)
        {
            var prop = GetRecordProperty(field);
            if (prop != null)
            {
                return SerializePropertyValue(record, prop);
            }

            if (record is IDictionary)
            {
                return (record as IDictionary)[field];
            }

            if (record is IList)
            {
                var findex = int.Parse(field);
                return (record as IList)[findex];
            }
            if (record is ITagedObject)
            {
                return (record as ITagedObject).Tags[field];
            }

            throw new KeyNotFoundException(string.Format(SysUtils.Strings.FieldNotFound, this.RecordType.Name + ". " + field));
        }

        // Convert serializable value to property: string, int, double, DateTime, Guid, byte[]
        protected internal virtual bool DeserializePropertyValue(object record, PropertyInfo prop, object value)
        {
            var val = value;
            if (DBNull.Value.Equals(val))
                val = null;

            // Enum
            if ((val is string) && (prop.PropertyType.IsEnum))
            {
                val = Enum.Parse(prop.PropertyType, val.ToString(), true);
            }

            // OneLineRecord 
            if ((val is string) && (prop.PropertyType.IsSubclassOf(typeof(SubRecord))))
            {
                var subRecord = Activator.CreateInstance(prop.PropertyType) as SubRecord;
                OneLineRecord.LoadFromString(subRecord, val as string);
                subRecord.OwnerRecord = record as Record;
                val = subRecord;
            }

            // Collection               
            if ((val is string) && (prop.PropertyType.IsGenericType) && (prop.PropertyType.GetGenericTypeDefinition() == typeof(SubRecords<>)))
            {
                var subRecType = prop.PropertyType.GetGenericArguments()[0];
                // new DbSubRecords(DbRecord rec)
                var subRecords = Activator.CreateInstance(prop.PropertyType, new object[] { record }) as System.Collections.IList;
                subRecords.LoadFromString(subRecType, val as string);
                val = subRecords;
            }

            prop.SetValue(record, val, null);

            return true;
        }

        // Return serializable value : string, int, double, DateTime, Guid, byte[]
        protected internal virtual object SerializePropertyValue(object record, PropertyInfo prop)
        {
            try
            {
                var propVal = prop.GetValue(record, null);
                if ((propVal == null) && (prop.PropertyType.IsValueType))
                    propVal = Activator.CreateInstance(prop.PropertyType);

                // Enum 
                if (prop.PropertyType.IsEnum)
                    return Enum.GetName(prop.PropertyType, propVal);

                // OneLineRecord 
                if (prop.PropertyType.IsSubclassOf(typeof(SubRecord)))
                    return OneLineRecord.SaveToString(propVal);

                if (propVal == null)
                    return null;

                // Collection         
                if ((prop.PropertyType.IsGenericType) && (prop.PropertyType.GetGenericTypeDefinition() == typeof(SubRecords<>)))
                {
                    var subRecords = propVal as System.Collections.IList;
                    return subRecords.SaveToString();
                }

                return propVal;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while getting value " + this.GetType().Name + "." + prop.Name + ": " + ex.Message, ex);
            }
        }


        public virtual Dictionary<string, object> GetRecordValues(object record)
        {
            var res = new Dictionary<string, object>();
            foreach (var prop in this.RecordProperties)
            {
                res[prop.Name] = this.SerializePropertyValue(record, prop);
            }
            return res;
        }
    }



    public abstract class RecordReader<T> : RecordProviderBase, IEnumerable<T>
    {
        public abstract IEnumerator<T> GetEnumerator();

        public RecordReader() 
            : base(typeof(T))
        {

        }

        Collections.IEnumerator Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


    public abstract class RecordWriter : RecordProviderBase
    {
        public RecordWriter(Type recType)
            : base(recType)
        {

        }
        protected abstract void AddToProvider(IEnumerable<Record> records);
    }


	//This same record could be used by DbRecordSet, MemoryRecordSet, AnyOtherRecordSet.
    public abstract class RecordProvider : RecordWriter
    {
        public PropertyInfo RecordIdProperty { get; private set; }

        internal RecordProvider(Type recType) : base(recType)
        {
            var idProps = RecordProperties.Where(p => p.IsDefined(typeof(RecordIdAttribute), true));
            if (idProps.Count() > 1)
                throw new Exception(RecordType.FullName + " contanis to many properties with " + typeof(RecordIdAttribute).Name);
            if (idProps.Count() < 1)
                throw new Exception(RecordType.FullName + " does not contain property with " + typeof(RecordIdAttribute).Name);
            RecordIdProperty = idProps.First();
        }


		// CRUD: Read
        public void Read(Record rec)
        {
            this.Read(new Record[] { rec });
        }
        public void Read(IEnumerable<Record> records)
        {
            records = records.Where(r => !r.RecordIsVirtual);
            LoadFromProvider(records);
            foreach (var rec in records)
            {
                rec.SetRecordSaved();
            }
        }
        protected abstract void LoadFromProvider(IEnumerable<Record> records);


		// CRUD: Create, Update
        public void Save(Record rec)
        {
            this.Save(new Record[] { rec });
        }
        public void Save(IEnumerable<Record> records)
        {
            var addList = new List<Record>();
            var updList = new List<Record>();
            foreach (var rec in records)
            {
                if (rec.RecordIsVirtual && rec.RecordIsChanged)
                    addList.Add(rec);
                else if (rec.RecordIsNew)
                    addList.Add(rec);
                else if (rec.RecordIsChanged)
                    updList.Add(rec);
                rec.RecordProvider = this;
            }

            AddToProvider(addList);
            WriteToProvider(updList);

            foreach (var rec in updList)
            {
                rec.SetRecordSaved();
            }
            foreach (var rec in addList)
            {
                var wasVirtual = rec.RecordIsVirtual;
                rec.SetRecordSaved();
                if (!wasVirtual)
                    rec.OnRecordInitialized();
            }
        }
        protected abstract void WriteToProvider(IEnumerable<Record> records);


		// CRUD: Delete
        public void Delete(Record rec)
        {
            this.Delete(new Record[] { rec });
        }
        public void Delete(IEnumerable<Record> records)
        {
            foreach (var rec in records)
            {
                rec.OnRecordDeleting();
            }
            DeleteFromProvider(records.Where(r=>!r.RecordIsVirtual));
            foreach (var rec in records)
            {
                rec.OnRecordDeleted();
            }
        }
        protected abstract void DeleteFromProvider(IEnumerable<Record> records);
    }

    public abstract class RecordProvider<T> : RecordProvider 
        where T : Record
    {
        public RecordProvider() : base(typeof(T))
        {
        }
    }


    /// <summary>
    /// Sample implementation of RecordProvider
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultRecordProvider<T> : RecordProvider<T>
        where T : Record
    {


        protected override void LoadFromProvider(IEnumerable<Record> records)
        {
        }

        protected override void AddToProvider(IEnumerable<Record> records)
        {
        }

        protected override void WriteToProvider(IEnumerable<Record> records)
        {
        }

        protected override void DeleteFromProvider(IEnumerable<Record> records)
        {
        }
    }

    public abstract class RecordSet : ILoadInfo
    {

        public RecordSet(RecordProvider recProvider)
        {
            this.Provider = recProvider;
        }

        public RecordProvider Provider { get; private set; }

        protected readonly System.Diagnostics.Stopwatch LoadingStopwatch = System.Diagnostics.Stopwatch.StartNew();
        public TimeSpan LoadTime
        {
            get { return LoadingStopwatch.Elapsed; }
        }

        public string LoadName
        {
            get { return Provider.RecordType.Name; }
        }

        public abstract void Save();

    }

    public class RecordSet<TRecord> : RecordSet, IEnumerable<TRecord>
        where TRecord : Record
    {

        private Dictionary<object, TRecord> Records = new Dictionary<object, TRecord>();

        public RecordSet() : this(new DefaultRecordProvider<TRecord>())
        {
        }

        public RecordSet(RecordProvider recProvider)
            : base(recProvider)
        {
            LoadingStopwatch.Stop();
        }

        public RecordSet(RecordReader<TRecord> reader, RecordProvider<TRecord> recProvider)
            : base(recProvider)
        {
            foreach (var rec in reader)
            {
                rec.RecordProvider = this.Provider;
                this.AddToDictionary(rec);
            }
            LoadingStopwatch.Stop();
        }

        public int Count
        {
            get { return Records.Count; }
        }

        public TRecord this[object recordId]
        {
            get
            {
                var res = default(TRecord);
                if (recordId == null)
                    return res;

                Records.TryGetValue(recordId, out res);
                return res;
            }
        }

        public TRecord FindOrAddVirtual(object recordId)
        {
            var res = this[recordId];
            if (res == null)
            {
                res = Activator.CreateInstance<TRecord>();
                this.Provider.RecordIdProperty.SetValue(res, recordId, null);
                this.AddVirtual(res);
            }
            return res;
        }

        protected void AddToDictionary(TRecord record)
        {
            object id = "GetRecrodId() failed!";
            try
            {
                id = record.GetRecordId();
                Records.Add(id, record);
                record.RecordDeleted += (s, r) => {
                    RemoveFromDictionary(record);
                };
            }
            catch (Exception ex)
            {
                throw new RecordException("Cannot add " + typeof(TRecord).Name + " (ID: " + id.ToText() + ") to " + this.GetType().Name, ex);
            }
        }

        private void RemoveFromDictionary(TRecord record)
        {
            var id = record.GetRecordId();
            Records.Remove(id);
        }

        /// <summary>
        /// Add record only to memory without saving it to database. That record will 
        /// be saved to database only if it is changed
        /// </summary>
        /// <param name="record"></param>
        public void AddVirtual(TRecord record)
        {
            record.SetRecordVirtual();
            record.RecordProvider = this.Provider;
            AddToDictionary(record);
            record.OnRecordInitialized();
        }


        public void Add(TRecord record)
        {
            Add(new TRecord[] { record });
        }

        public void Add(IEnumerable<TRecord> records)
        {
            this.Provider.Save(records.Cast<Record>());

            foreach (var rec in records)
            {
                AddToDictionary(rec);
            }
        }

        public override void Save()
        {
            Provider.Save(Records.Values.Cast<Record>());
        }

        public void Delete(IEnumerable<TRecord> records)
        {
            this.Provider.Delete(records.Cast<Record>());
        }

        public void Clear()
        {
            this.Provider.Delete(Records.Values.Cast<Record>());
        }

        IEnumerator<TRecord> IEnumerable<TRecord>.GetEnumerator()
        {
            return Records.Values.GetEnumerator();
        }

        Collections.IEnumerator Collections.IEnumerable.GetEnumerator()
        {
            return (Records.Values as Collections.IEnumerable).GetEnumerator();
        }
    }

    public class RecordContext
    {


        public IEnumerable<RecordSet> GetRecordSets()
        {
            var props = this.GetType().GetProperties();
            var fields = this.GetType().GetFields();
            var res = new List<RecordSet>();

            foreach (var p in props)
            {
                if (p.PropertyType.IsSubclassOf(typeof(RecordSet)))
                {
                    var r = p.GetValue(this, null) as RecordSet;
                    if (r != null)
                        res.Add(r);
                }
            }
            foreach (var f in fields)
            {
                if (f.FieldType.IsSubclassOf(typeof(RecordSet)))
                {
                    var r = f.GetValue(this) as RecordSet;
                    if (r != null)
                        res.Add(r);
                }
            }
            return res;
        }

        public virtual void Save()
        {
            var sets = GetRecordSets();
            foreach (var rs in sets)
	        {
                rs.Save();
	        }
        }
    }

    public static class RecordsetEx
    {
        public static RecordsView<T> Filter<T>(this IEnumerable<T> items, string filter)
            where T : Record
        {
            return items.Where(r => r.FullSearchStringContains(filter)).ToView();
        }
    }

}
