using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{

    public static class StringBuilderExt
    {

        public static StringBuilder AppendFormatLine(this StringBuilder sb, string format, params object[] args)
        {
            sb.AppendFormat(format, args);
            sb.AppendLine();
            return sb;
        }




        /// <summary>
        /// Determines whether the beginning of this string matches the specified string.
        //  Coprasion uses CurrentCulture with IgnoreCase.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool StartsWith(this StringBuilder sb, params string[] s)
        {
            return sb.StartsWith(true, s);
        }

        public static bool StartsWith(this StringBuilder sb, bool ignoreCase, params string[] s)
        {
            var sbs = sb.ToString();
            foreach (string sItem in s)
                if (sbs.StartsWith(ignoreCase, sItem))
                    return true;
            return false;
        }
        public static bool EndsWith(this StringBuilder sb, params string[] s)
        {
            return sb.EndsWith(true, s);
        }

        public static bool EndsWith(this StringBuilder sb, bool ignoreCase, params string[] s)
        {
            var sbs = sb.ToString();

            foreach (string sItem in s)
                if (sbs.EndsWith(ignoreCase, sItem))
                    return true;
            return false;
        }

        public static bool Contains(this StringBuilder sb, params char[] chars)
        {
            for (int i = 0; i < sb.Length; i++)
            {
                if (chars.Contains(sb[i]))
                    return true;
            }
            return false;
        }


        #region *** TrimStart() ***

        public static StringBuilder TrimStart(this StringBuilder source, bool ignoreCase, params string[] s)
        {
            foreach (var sItem in s)
            {
                if (source.StartsWith(ignoreCase, s))
                {
                    source.Remove(0, sItem.Length);
                }
            }
            return source;
        }

        public static StringBuilder TrimStart(this StringBuilder source, params string[] sArr)
        {
            return source.TrimStart(false, sArr);
        }

        public static StringBuilder TrimStart(this StringBuilder source, params char[] chArr)
        {
            if ((source.Length > 0) && (chArr.Contains(source[0])))
            {
                source.Remove(0, 1);
                return source.TrimStart(chArr);
            }
            else
            {
                return source;
            }
        }

        public static StringBuilder TrimStart(this StringBuilder source)
        {
            if ((source.Length > 0) && (char.IsWhiteSpace(source[0])))
            {
                source.Remove(0, 1);
                return source.TrimStart();
            }
            else
            {
                return source;
            }
        }

        #endregion



        #region *** TrimEnd() ***

        public static StringBuilder TrimEnd(this StringBuilder source, bool ignoreCase, params string[] s)
        {
            foreach (var sItem in s)
            {
                var srcStr = source.ToString();
                if (srcStr.EndsWith(ignoreCase, s))
                    source.Remove(srcStr.LastIndexOf(sItem), sItem.Length);
            }
            return source;
        }

        public static StringBuilder TrimEnd(this StringBuilder source, params string[] sArr)
        {
            return source.TrimEnd(false, sArr);
        }

        public static StringBuilder TrimEnd(this StringBuilder source, params char[] chArr)
        {
            var lastIndex = source.Length - 1;
            if ((source.Length > 0) && (chArr.Contains(source[lastIndex])))
            {
                source.Remove(lastIndex, 1);
                return source.TrimEnd(chArr);
            }
            else
            {
                return source;
            }
        }

        public static StringBuilder TrimEnd(this StringBuilder source)
        {
            var lastIndex = source.Length - 1;
            if ((source.Length > 0) && (char.IsWhiteSpace(source[lastIndex])))
            {
                source.Remove(lastIndex, 1);
                return source.TrimEnd();
            }
            else
            {
                return source;
            }
        }

        #endregion



        #region *** Trim() ***

        public static StringBuilder Trim(this StringBuilder source, bool ignoreCase, params string[] s)
        {
            source.TrimStart(ignoreCase, s);
            source.TrimEnd(ignoreCase, s);
            return source;
        }

        public static StringBuilder Trim(this StringBuilder source, params string[] sArr)
        {
            source.TrimStart(sArr);
            source.TrimEnd(sArr);
            return source;
        }

        public static StringBuilder Trim(this StringBuilder source, params char[] chArr)
        {
            source.TrimStart(chArr);
            source.TrimEnd(chArr);
            return source;
        }

        public static StringBuilder Trim(this StringBuilder source)
        {
            source.TrimStart();
            source.TrimEnd();
            return source;
        }

        #endregion


        public static StringBuilder TrimAfter(this StringBuilder sb, string s)
        {
            var index = sb.ToString().IndexOf(s);
            if (index >= 0)
                sb.Remove(index, s.Length);
            return sb;

        }

        public static StringBuilder Replace(this StringBuilder sb, string oldValue, string newValue)
        {
            var s = sb.ToString();
            var sNew = s.Replace(oldValue, newValue);
            if (s != sNew)
            {
                sb.Remove(0, sb.Length); //.Clear();
                sb.Append(s.Replace(oldValue, newValue));
            }
            return sb;
        }


        public static StringBuilder Remove(this StringBuilder sb, string s)
        {
            return sb.Replace(s, "");
        }

        public static string Substring(this StringBuilder sb, int startIndex, int endIndex)
        {
            var res = new StringBuilder();
            for (int i = startIndex; i <= endIndex; i++)
            {
                res.Append(sb[i]);
            }
            return res.ToString();
        }

        public static string Extract(this StringBuilder sb, int startIndex, int endIndex)
        {
            var res = sb.Substring(startIndex, endIndex);
            sb.Remove(startIndex, endIndex - startIndex + 1);
            return res;
        }

        public static string Extract(this StringBuilder sb, int startIndex)
        {
            var res = sb.Substring(startIndex, sb.Length - 1);
            sb.Remove(startIndex, sb.Length - 1);
            return res;
        }

        public static string ExtractAfter(this StringBuilder sb, bool ignoreCase, string s)
        {
            string res = String.Empty;
            int startIndex = sb.ToString().IndexOf(s, StringEx.GetStringComparison(ignoreCase));
            if (startIndex > -1)
            {
                res = sb.Extract(startIndex);
                // res = "Date=02.02.2002"
                // delete "Date="
                res = res.Remove(0, s.Length);
            }
            return res;
        }

        public static string ExtractAfter(this StringBuilder sb, string s)
        {
            return sb.ExtractAfter(false, s);
        }

        public static string ExtractBefore(this StringBuilder sb, bool ignoreCase, string s)
        {
            string res = String.Empty;
            int startIndex = sb.ToString().IndexOf(s, StringEx.GetStringComparison(ignoreCase));
            if (startIndex > 0)
            {
                res = sb.Extract(0, startIndex - 1);
            }
            return res;
        }
        public static string ExtractBefore(this StringBuilder sb, string s)
        {
            return sb.ExtractBefore(false, s);
        }

        public static string ExtractBetween(this StringBuilder sb, bool ignoreCase, string sLeft, string sRight)
        {
            string res = String.Empty;
            var s = sb.ToString();
            int pLeft = s.IndexOf(sLeft, StringEx.GetStringComparison(ignoreCase));
            int pRight = s.IndexOf(sRight, pLeft + 1, StringEx.GetStringComparison(ignoreCase));
            if ((pLeft > -1) && (pRight > pLeft + sLeft.Length))
            {
                res = sb.Extract(pLeft, pRight);
                // res = "<p>Title</p>"
                // delete "<p>" i "</p>"
                res = res.Remove(0, sLeft.Length);
                res = res.Remove(res.LastIndexOf(sRight));
            }
            return res;
        }

        public static string ExtractBetween(this StringBuilder sb, string sLeft, string sRight)
        {
            return sb.ExtractBetween(false, sLeft, sRight);
        }

        public static string[] Split(this StringBuilder sb, bool removeEmptyEntries, params string[] separators)
        {
            if (removeEmptyEntries)
                return sb.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);
            else
                return sb.ToString().Split(separators, StringSplitOptions.None);
        }

        public static string[] Split(this StringBuilder sb)
        {
            return sb.Split(true, null);
        }


        private class ValuePos
        {
            public int StartIndex = -1;
            public int EndIndex = -1;
        }

        private static ValuePos _firstValuePos(StringBuilder sb, params char[] separators)
        {
            var res = new ValuePos();

            if (sb.Length < 1)
                return res;

            res.StartIndex = 0;

            // omit beginning spaces: "   aaa bbb ccc"
            while (separators.Contains(sb[res.StartIndex]) && (res.StartIndex < sb.Length))
            {
                res.StartIndex++;
            }

            // Empty string without words "   "
            if ((res.StartIndex == sb.Length) && separators.Contains(sb[res.StartIndex]))
            {
                res.StartIndex = -1;
                return res;
            }

            // It could be one-char-word: "    A   and another words  "
            res.EndIndex = res.StartIndex;

            // "   A"
            if (res.EndIndex == sb.Length - 1)
                return res;

            // search for ending spaces: "aaa   "
            while (!separators.Contains(sb[res.EndIndex]) && (res.EndIndex < sb.Length))
            {
                res.EndIndex++;
            }

            return res;
        }

        private static ValuePos _lastValuePos(StringBuilder sb, params char[] separators)
        {
            var res = new ValuePos();

            if (sb.Length < 1)
                return res;

            res.EndIndex = sb.Length - 1;

            // omit ending spaces: "aaa bbb ccc   "
            while (separators.Contains(sb[res.EndIndex]) && (res.EndIndex >= 0))
            {
                res.EndIndex--;
            }

            // Empty string without words "   "
            if ((res.EndIndex == 0) && separators.Contains(sb[0]))
            {
                res.EndIndex = -1;
                return res;
            }

            // "Another words and an A"
            res.StartIndex = res.EndIndex;

            // "A   "
            if (res.StartIndex == 0)
                return res;

            // search for starting spaces: "   aaa"
            while (separators.Contains(sb[res.StartIndex]) && (res.StartIndex >= 0))
            {
                res.StartIndex--;
            }

            return res;
        }

        public static string ExtractFirstValue(this StringBuilder sb, params char[] separators)
        {
            var wPos = _firstValuePos(sb, separators);
            if (wPos.StartIndex < 0)
                throw new Exception(string.Format("String '{0}' contains no separated values.", sb));
            return sb.Extract(wPos.StartIndex, wPos.EndIndex);
        }

        public static string ExtractLastValue(this StringBuilder sb, params char[] separators)
        {
            var wPos = _lastValuePos(sb, separators);
            if (wPos.StartIndex < 0)
                throw new Exception(string.Format("String '{0}' contains no separated values.", sb));
            return sb.Extract(wPos.StartIndex, wPos.EndIndex);
        }

    }
}
