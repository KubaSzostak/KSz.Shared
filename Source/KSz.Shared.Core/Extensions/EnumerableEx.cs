using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{


    public static class EnumerableExtensions
    {

        public static List<string> ToKeyValueTexts(this IDictionary dict, string keyValueSeparator)
        {
            var res = new List<string>();
            foreach (var key in dict.Keys)
            {
                var val = dict[key].ToText();
                res.Add(key.ToText() + keyValueSeparator + val);
            }
            return res;
        }

        public static string ToKeyValueText(this IDictionary dict, string keyValueSeparator, string itemSeparator)
        {
            return dict.ToKeyValueTexts(keyValueSeparator).Join(itemSeparator);
        }


        public static string ToKeyValueText(this IDictionary dict)
        {
            return dict.ToKeyValueText(": ", "\r\n");
        }

        public static List<TResultItem> ConvertTo<TResultItem>(this IEnumerable list)
        {
            List<TResultItem> res = new List<TResultItem>();
            foreach (var item in list)
            {
                if (typeof(TResultItem) == typeof(string))
                {
                    object itemObj = (object)item.ToText();
                    res.Add((TResultItem)itemObj);
                }
                else
                    res.Add((TResultItem)item);
            }
            return res;
        }

        public static void AddListItem<TKey, TListItem>(this Dictionary<TKey, List<TListItem>> dict, TKey key, TListItem item)
        {
            List<TListItem> list;
            if (!dict.TryGetValue(key, out list))
            {
                list = new List<TListItem>();
                dict.Add(key, list);
            }
            list.Add(item);
        }

        public static List<TListItem> GetList<TKey, TListItem>(this Dictionary<TKey, List<TListItem>> dict, TKey key)
        {
            List<TListItem> list;
            if (!dict.TryGetValue(key, out list))
            {
                list = new List<TListItem>(); // Zwróć pustą listę
            }
            return list;
        }

        public static void RemoveListItem<TKey, TListItem>(this Dictionary<TKey, List<TListItem>> dict, TKey key, TListItem item)
        {
            if (!dict.ContainsKey(key))
                return;
            var list = dict.GetList(key);
            if (list.Contains(item))
                list.Remove(item);
        }

        public static Dictionary<TKey, TSource[]> ToLookupDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            Dictionary<TKey, TSource[]> res = new Dictionary<TKey, TSource[]>();
            var lookup = source.ToLookup(keySelector);
            foreach (var item in lookup)
            {
                res[item.Key] = item.ToArray();
            }
            return res;
        }

        public static List<TSource> ToSortedList<TSource>(this IEnumerable<TSource> source)
        {
            List<TSource> list = source.ToList();
            list.Sort();
            return list;
        }

        public static List<TSource> Sort<TSource>(this IEnumerable<TSource> source) where TSource : IComparable<TSource>
        {
            var list = source as List<TSource>;
            if (list == null)
                list = source.ToList();
            list.Sort();
            return list;
        }
        public static List<TSource> SortDescending<TSource>(this IEnumerable<TSource> source) where TSource : IComparable<TSource>
        {
            var list = source.Sort();
            list.Reverse();
            return list;
        }

        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source)
        {
            return new HashSet<TSource>(source);
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }

        public static void ForReverse<T>(this IList<T> items, Action<T> action)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                T item = items[i];
                action(item);
            }
        }


        public static void AddItems<TSource>(this ICollection<TSource> source, params TSource[] items)
        {
            foreach (var item in items)
            {
                source.Add(item);
            }
        }

        public static void AddItems<TSource>(this ICollection<TSource> source, IEnumerable<TSource> items)
        {
            foreach (var item in items)
            {
                source.Add(item);
            }
        }

        public static void Assign<TSource>(this IList<TSource> thisList, IEnumerable<TSource> src)
        {
            thisList.Clear();
            foreach (TSource item in src)
                thisList.Add(item);
        }

        public static void AddIfNotContains<T>(this ICollection<T> items, T item)
        {
            if (!items.Contains(item))
                items.Add(item);
        }

        public static IEnumerable<LinkedListNode<T>> GetNodeEnumerator<T>(this LinkedList<T> list)
        {
            var current = list.First;
            while (current != null)
            {
                yield return current;
                current = current.Next;
            }
        }

        public static string Join(this IEnumerable<string> list, string separator)
        {
            return string.Join(separator, list.ToArray());
        }

        public static void AddFormat(this ICollection<string> list, string format, params object[] args)
        {
            list.Add(string.Format(format, args));
        }

        public static TSource FirstOrDefault<TSource>(this IList<TSource> source)
        {
            if (source.Count > 0)
                return source[0];
            else
                return default(TSource);
        }

        public static TSource LastOrDefault<TSource>(this IList<TSource> source)
        {
            if (source.Count > 0)
                return source[source.Count - 1];
            else
                return default(TSource);
        }


        public static void RemoveValues<T>(this IList<T> thisList, params T[] values)
        {
            int valCount = values.Length;
            for (int i = thisList.Count - 1; i > -1; i--)
                for (int vali = 0; vali < valCount; vali++)
                {
                    if (thisList[i].Equals(values[vali]))
                        thisList.RemoveAt(i);
                }
        }

        public static void RemoveEmptyValues<T>(this IList<T> thisList)
        {
            for (int i = thisList.Count - 1; i > -1; i--)
            {
                if (thisList[i].IsEmpty())
                    thisList.RemoveAt(i);
            }
        }

        public static void RemoveDuplicates<T>(this IList<T> thisList)
        {
            RemoveEmptyValues(thisList);
            for (int i = thisList.Count - 1; i > -1; i--)
            {
                for (int j = 0; j < i; j++)
                {
                    if (thisList[i].Equals(thisList[j]))
                        thisList.RemoveAt(i);
                }
            }
        }

        public static T ExtractAt<T>(this IList<T> thisList, int i)
        {
            T res = thisList[i];
            thisList.RemoveAt(i);
            return res;
        }

        public static T ExtractFirst<T>(this IList<T> thisList)
        {
            return thisList.ExtractAt(0);
        }

        public static List<T> Extract<T>(this IList<T> list, Func<T, bool> predicate)
        {
            var res = new List<T>();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    res.Add(list[i]);
                    list.RemoveAt(i);
                }
            }
            res.Reverse();
            return res;
        }

        public static List<T> Extract<T>(this IList<T> list, int from, int to)
        {
            var res = new List<T>();
            for (int i = to - 1; i >= from; i--)
            {
                res.Add(list[i]);
                list.RemoveAt(i);
            }
            res.Reverse(); // items was added from the end
            return res;
        }

        public static void TrimAll(this IList<string> thisList)
        {
            for (int i = 0; i < thisList.Count; i++)
            {
                thisList[i] = thisList[i].Trim();
            }
        }

        public static int FindStartsWith(this IList<string> thisList, string Value)
        {
            for (int i = 0; i < thisList.Count; i++)
                if (thisList[i].StartsWith(Value))
                    return i;
            return -1;
        }

        public static int FindLastStartsWith(this IList<string> thisList, string Value)
        {
            for (int i = thisList.Count - 1; i >= 0; i--)
                if (thisList[i].StartsWith(Value))
                    return i;
            return -1;
        }

    }
}
