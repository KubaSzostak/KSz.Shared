using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System
{

    public static class Extensions
    {

        public static bool IsEmpty(this object value)
        {
            if (value == null)
                return true;

            //if (value == DBNull.Value)
            //    return true;
            
            if (value is string)
                return string.IsNullOrEmpty((value as string));

            return false;
        }

        public static string ToText(this object o, string emptyText)
        {
            if (o.IsEmpty())
                return emptyText;

            return o.ToString();
        }

        public static string ToText(this object o)
        {
            return o.ToText("");
        }

        public static T ValueOrDefault<T>(this T value, T defaultValue) where T : class
        {
            if (value != null)
                return value;
            else
                return defaultValue;
        }



        public static bool IsIn<T>(this T ths, params T[] sArr)
        {
            if (sArr == null)
                return false;
            foreach (var s in sArr)
            {
                if (object.Equals(ths, s))
                    return true;
            }
            return false;
        }

        public static bool ToBool(this object value)
        {
            if (value.IsEmpty())
                return false;
            if (value is string)
            {
                var sval = value.ToText();
                if (sval.ToLower().IsIn("", "0", "false", "fałsz", "no", "n", "nie"))
                    return false;
                else if (sval.ToLower().IsIn("1", "true", "prawda", "yes", "y", "tak"))
                    return true;
                throw new InvalidCastException("Cannot convert '" + sval + "' to Boolean value");
            }
            return (bool)value;
        }

        public static int CompareTo<T>(this Nullable<T> thisVal, Nullable<T> otherVal) where T : struct
        {
            if ((thisVal.HasValue) && (!otherVal.HasValue))
                return 1;
            if ((!thisVal.HasValue) && (otherVal.HasValue))
                return -1;

            if (thisVal.Value is IComparable<T>)
                return (thisVal.Value as IComparable<T>).CompareTo(otherVal.Value);

            return 0;
        }

        /// <summary>
        /// For Anonymous Types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemOftype"></param>
        /// <returns></returns>
        public static List<T> NewList<T>(this T itemOftype)
        {
            List<T> newList = new List<T>();
            return newList;
        }

    }
}
