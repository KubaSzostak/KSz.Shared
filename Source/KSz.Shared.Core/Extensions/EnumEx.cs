using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace System
{
    public static class EnumEx
    {
        public static T ToEnum<T>(this string s) where T : struct
        {
            return (T)Enum.Parse(typeof(T), s, true);
        }


        public static IEnumerable<Nullable<T>> GetNullableEnumValues<T>() where T : struct
        {
            var enumValues = Enum.GetValues(typeof(T)).Cast<T>();

            var res = new List<Nullable<T>>();
            res.Add(null);
            foreach (var e in enumValues)
                res.Add(e);

            return res;
        }

        public static IEnumerable<T> GetEnumValues<T>() where T : struct
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
