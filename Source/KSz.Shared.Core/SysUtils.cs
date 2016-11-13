using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace System
{
    public class ListProvider
    {
        virtual protected IList<T> CreateList<T>()
        {
            return new List<T>();
        }

        virtual protected IList<T> CreateList<T>(IEnumerable<T> items)
        {
            return new List<T>(items);
        }


        public static ListProvider Provider = new ListProvider();

        public static IList<T> Create<T>()
        {
            return Provider.CreateList<T>();
        }

        public static IList<T> Create<T>(IEnumerable<T> items)
        {
            return Provider.CreateList(items);
        }

    }

    public class SysUtils
    {
        public static LocalizationStrings Strings = new LocalizationStrings();
        public static readonly Random Random = new Random();


        static SysUtils()
        {
            
        }

        public static void Init()
        {
        }

        public static T FromInvariantText<T>(string value)
        {
            if (value != null)
                value = value.Trim();

            if (!string.IsNullOrEmpty(value))
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            else
                return default(T);
        }

        public static string ToInvariantText(object value)
        {
            return Convert.ChangeType(value, typeof(string), CultureInfo.InvariantCulture).ToString();
        }

        public static Regex GetRegEx(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                pattern = "*";

            string[] patterns = pattern.Split(',');
            for (int i = 0; i < patterns.Length; i++)
            {
                string s = patterns[i].Trim().Replace("*", ".+?");
                patterns[i] = @"\A" + s + @"\Z";
            }

            return new Regex(string.Join("|", patterns));
        }


        public static bool SameExt(string fileName, string ext)
        {
            return fileName.EndsWith(ext, StringComparison.CurrentCultureIgnoreCase);
        }

        private static string GetFirstNotEmpty(params string[] values)
        {
            foreach (var v in values)
            {
                if (!string.IsNullOrEmpty(v))
                    return v;
            }
            return null;
        }

        public static T GetFirstNotDefault<T>(params T[] values)
        {
            foreach (var item in values)
            {
                if (!object.Equals(item, default(T)))
                    return item;
            }
            return default(T);
        }


        // https://msdn.microsoft.com/en-us/library/system.text.unicodeencoding.getpreamble(v=vs.110).aspx
        // http://stackoverflow.com/questions/1317700/strip-byte-order-mark-from-string-in-c-sharp
        // https://en.wikipedia.org/wiki/Byte_order_mark
        // Pay attention to difference between UTF-8 and UTF-16


        // UTF-8: EF BB BF (representation of the BOM is the byte sequence)
        public static readonly string ByteOrderMarkUtf8 = new string(new char[] { '\u00EF', '\u00BB', '\u00BF' }); // char in a C# string defaults to the UTF-16 encoding of Unicode, which is 2 bytes


        // UTF-16 Big endian byte order:    FE FF
        // UTF-16 Little endian byte order: FF FE
        // (representation of the BOM is one of byte sequence)
        public static readonly char[] ByteOrderMarkUtf16 = new char[] { '\uFEFF', '\uFFFE' }; // Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        public static string TrimBom(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            s = s.TrimStart(ByteOrderMarkUtf16);
            if (s.StartsWith(ByteOrderMarkUtf8))
                s = s.Substring(0 + ByteOrderMarkUtf8.Length);

            return s;
        }

    }
}
