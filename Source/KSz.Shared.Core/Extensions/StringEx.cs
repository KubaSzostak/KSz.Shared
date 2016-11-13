using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace System
{
    public static class StringEx
    {
        internal static StringComparison GetStringComparison(bool ignoreCase)
        {
            if (ignoreCase)
                return StringComparison.CurrentCultureIgnoreCase;
            else
                return StringComparison.CurrentCulture;
        }

        #region *** Starts/Ends With ***

        /// <summary>
        /// Determines whether the beginning of this string matches the specified string.
        //  Coprasion uses CurrentCulture with IgnoreCase.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool StartsWith(this string source, params string[] s)
        {
            return source.StartsWith(false, s);
        }

        public static bool StartsWith(this string source, bool ignoreCase, params string[] s)
        {
            foreach (string sItem in s)
                if (source.StartsWith(sItem, GetStringComparison(ignoreCase)))
                    return true;
            return false;
        }



        public static bool EndsWith(this string source, params string[] s)
        {
            return source.StartsWith(false, s);
        }

        public static bool EndsWith(this string source, bool ignoreCase, params string[] s)
        {
            foreach (string sItem in s)
                if (source.EndsWith(sItem, GetStringComparison(ignoreCase)))
                    return true;
            return false;
        }


        #endregion


        public static string NextPointId(this string pointId)
        {
            string fmt = "";
            string prefix = "";
            string number = "";
            for (int i = pointId.Length - 1; i >= 0; i--)
            {
                if (char.IsDigit(pointId[i]))
                {
                    fmt += "0";
                    number = pointId[i] + number;
                }
                else
                {
                    prefix = pointId.Substring(0, i + 1);
                    break;
                }
            }

            if (string.IsNullOrEmpty(number))
                number = "0";
            try
            {
                int nextNumber = Convert.ToInt32(number) + 1;
                return prefix + nextNumber.ToString(fmt);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("NextPointId() failed:");
                Debug.WriteLine(ex.Message);
                return pointId + "1";
            }
        }

        public static string RemoveDoubleSpaces(this string s)
        {
            s = s.Trim(' ');
            while (s.IndexOf("  ") > -1)
            {
                s = s.Replace("  ", " ");
            }
            return s.Trim();
        }

        public static bool TryParse(this string s, ref double result)
        {
            if (s != null)
                s = s.Trim();

            if (string.IsNullOrEmpty(s))
            {
                return true;
            }

            // Always use InvariantCulture
            if (NumberFormatInfo.CurrentInfo.NumberDecimalSeparator == ",")
                s = s.Replace(",", NumberFormatInfo.InvariantInfo.NumberDecimalSeparator);

            double parseRes = 0.0;
            if (double.TryParse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out parseRes))
            {
                result = parseRes;
                return true;
            }
            else
            {
                //result = double.NaN;
                return false;
            }
        }

        public static double ToDouble(this string s, double defaultValue)
        {
            double res = defaultValue;
            if (!s.TryParse(ref res))
                throw new FormatException(string.Format(SysUtils.Strings.InvalidNumberFormatX, s));
            return res;
        }

        public static double ToDouble(this string s)
        {
            return s.ToDouble(0.0);
        }

        public static bool ToInt(this string s, int defaultValue, out int result)
        {
            if (s != null)
                s = s.Trim();

            if (string.IsNullOrEmpty(s))
            {
                result = defaultValue;
                return true;
            }

            return int.TryParse(s, out result);
        }

        public static int ToInt(this string s, int defaultValue)
        {
            int res;
            if (!s.ToInt(defaultValue, out res))
                throw new FormatException(string.Format(SysUtils.Strings.InvalidNumberFormatX, s));
            return res;
        }

        public static int ToInt(this string s)
        {
            return s.ToInt(0);
        }

        public static bool IsInt(this string value)
        {            
            int res;
            return value.ToInt(0, out res);
        }


        public static bool IsDouble(this string value)
        {
            double res = 0;
            return value.TryParse(ref res);
        }

        public static string RemoveFirstChars(this string s, int count)
        {
            return s.Remove(0, count);
        }

        public static string RemoveLastChars(this string s, int count)
        {
            return s.Remove(s.Length - count, count);
        }

        public static string RemoveChars(this string s, params char[] chars)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            string res = s;
            foreach (var ch in chars)
            {
                res.Replace(ch.ToString(), "");
            }
            return res;
        }

        public static string FirstChars(this string s, int count)
        {
            return s.Substring(0, count);
        }

        public static string LastChars(this string s, int count)
        {
            return s.Substring(s.Length - count, count);
        }

        public static string[] SplitValues(this string s, bool removeEmptyEntries, params string[] separator)
        {
            if (s.IsEmpty())
                return new string[0];

            StringSplitOptions opt = StringSplitOptions.None;
            if (removeEmptyEntries)
                opt = StringSplitOptions.RemoveEmptyEntries;

            return s.Split(separator, opt);
        }

        public static string Quote(this string s, string quoteChar)
        {
            return quoteChar + s + quoteChar;
        }

        public static bool SameText(this string ths, string s)
        {
            if (ths == null)
                return ths == s;
            return ths.Equals(s, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool SameText(this string ths, params string[] sArr)
        {
            if (ths == null)
                return false;
            if (sArr == null)
                return false;
            foreach (var s in sArr)
            {
                if (ths.Equals(s, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        public static string StringBefore(this string s, string terminator)
        {
            int pos = s.IndexOf(terminator);
            return s.Substring(0, pos);
        }

        public static string StringAfter(this string s, string terminator)
        {
            int pos = s.IndexOf(terminator);
            return s.Substring(pos+1);
        }

        public static string StringAfterLast(this string s, string terminator)
        {
            int pos = s.LastIndexOf(terminator);
            return s.Substring(pos + 1);
        }
    }
}
