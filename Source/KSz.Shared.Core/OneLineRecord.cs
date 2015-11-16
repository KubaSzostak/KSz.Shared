using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System
{


    [AttributeUsage(AttributeTargets.Property)]
    public class OneLineRecordFieldAttribute : Attribute
    {
    }



    public static class OneLineRecord
    {

        public class KeyValue
        {
            public KeyValue(string k, string v)
            {
                Key = k;
                Value = v;
            }

            public string Key;
            public string Value;

            public override string ToString()
            {
                return string.Format("{0}: {1}", Key, Value);
            }
        }

        public class KeyValueList : List<KeyValue>
        {
            public void Add(string key, string value)
            {
                this.Add(new KeyValue(key, value));
            }

            public KeyValue this[string key]
            {
                get
                {
                    foreach (var kv in this)
                    {
                        if (kv.Key == key)
                            return kv;
                    }
                    return null;
                }
            }
        }


        public static string ValueSeparator = "꞉ ";
        public static string FieldSeparator = " □ ";  //⁞‖‖║∙gA▪□ ■ ▫◊꞊꞉
        public static string RecordSeparator = " ║ ";

        private static string[] ValueSeparatorArray = new string[] { ValueSeparator };
        private static string[] FieldSeparatorArray = new string[] { FieldSeparator };

        public static KeyValueList GetValues(string s)
        {
            var res = new KeyValueList();
            if (string.IsNullOrEmpty(s))
                return res;

            var fields = s.Split(FieldSeparatorArray, StringSplitOptions.None);
            foreach (var fld in fields)
            {
                //fld = "name꞉ Kuba";
                var keyValArr = fld.Split(ValueSeparatorArray, StringSplitOptions.None);
                if (keyValArr.Length == 2)
                    res.Add(keyValArr[0], keyValArr[1]);
            }

            return res;
        }

        public static string GetString(KeyValueList values)
        {
            var res = new List<string>();
            foreach (var kv in values)
            {
                string fld = kv.Key + ValueSeparator + kv.Value;
                res.Add(fld);
            }
            return res.Join(FieldSeparator);
        }


        private static Dictionary<Type, IEnumerable<PropertyInfo>> Properties = new Dictionary<Type, IEnumerable<PropertyInfo>>();

        internal static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            IEnumerable<PropertyInfo> res;
            if (Properties.TryGetValue(type, out res))
                return res;

            res = type.GetProperties().Where(p => p.IsDefined(typeof(OneLineRecordFieldAttribute), true)).Where(p => p.CanWrite);
            Properties[type] = res;

            return res;
        }

        public static void LoadFromString(object record, string s)
        {
            var values = OneLineRecord.GetValues(s);
            var props = OneLineRecord.GetProperties(record.GetType());

            foreach (var prop in props)
            {
                var kv = values[prop.Name];
                if (kv != null)
                {
                    prop.SetStringValue(record, kv.Value);
                    if (record is ObservableObject)
                        (record as ObservableObject).OnPropertyChanged(prop.Name);
                }
            }
        }

        public static string SaveToString(object record)
        {

            var values = new OneLineRecord.KeyValueList();
            var props = OneLineRecord.GetProperties(record.GetType());

            foreach (var prop in props)
            {
                values.Add(prop.Name, prop.GetStringValue(record));
            }
            return OneLineRecord.GetString(values);
        }

    }

    public class OneLineRecord<T>
    {
        public static void LoadFromString(T record, string s)
        {
            OneLineRecord.LoadFromString(record, s);
        }

        public static T FromString(string s)
        {
            T res = Activator.CreateInstance<T>();
            LoadFromString(res, s);
            return res;
        }

        public static string SaveToString(T record)
        {
            return OneLineRecord.SaveToString(record);
        }
    }

    public static class OneLineRecordsetExtensions
    {
        public static string SaveToString(this IEnumerable recordset)
        {
            List<string> res = new List<string>();
            foreach (var rec in recordset)
            {
                res.Add(OneLineRecord.SaveToString(rec));
            }
            return res.Join(OneLineRecord.RecordSeparator);
        }

        public static void LoadFromString(this IList recordset, Type recType, string s)
        {
            recordset.Clear();
            if (string.IsNullOrEmpty(s))
                return;

            var res = new Dictionary<string, string>();
            var records = s.Split(new string[] { OneLineRecord.RecordSeparator }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var recStr in records)
            {
                var rec = Activator.CreateInstance(recType);
                OneLineRecord.LoadFromString(rec, recStr);
                recordset.Add(rec);
            }
        }

        public static void LoadFromString<T>(this ICollection<T> recordset, string s)
        {
            recordset.Clear();
            if (string.IsNullOrEmpty(s))
                return;

            var res = new Dictionary<string, string>();
            var records = s.Split(new string[] { OneLineRecord.RecordSeparator }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var recStr in records)
            {
                T rec = Activator.CreateInstance<T>();
                OneLineRecord<T>.LoadFromString(rec, recStr);
                recordset.Add(rec);
            }
        }

        // public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector);

        public static string SaveToString<TRecord, TKey, TValue>(this IEnumerable<TRecord> recordset, Func<TRecord, TKey> keySelector, Func<TRecord, TValue> valueSelector)
        {
            var values = new OneLineRecord.KeyValueList();

            foreach (var rec in recordset)
            {
                var keyStr = SysUtils.ToInvariantText(keySelector(rec));
                var valStr = SysUtils.ToInvariantText(valueSelector(rec));

                values.Add(keyStr, valStr);
            }
            return OneLineRecord.GetString(values);
        }


        public static void LoadFromString<TRecord, TKey, TValue>(this ICollection<TRecord> recordset, string s, Expression<Func<TRecord, TKey>> keyProperty, Expression<Func<TRecord, TValue>> valueProperty)
        {
            recordset.Clear();
            var keyPropInfo = keyProperty.GetPropertyInfo();// Extensions.GetPropertyInfo<TRecord, TKey>(keyProperty);
            var valPropInfo = valueProperty.GetPropertyInfo();

            var values = OneLineRecord.GetValues(s);
            foreach (var kv in values)
            {
                TRecord rec = Activator.CreateInstance<TRecord>();
                keyPropInfo.SetStringValue(rec, kv.Key);
                valPropInfo.SetStringValue(rec, kv.Value);
                recordset.Add(rec);
            }
        }

    }

}
