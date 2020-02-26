using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Jetstream {
    public static class Compat {
        public static readonly Random Random = new Random ();

        public static double random () {
            return Random.NextDouble ();
        }

        public static void push<T> (this List<T> list, T item) {
            list.Add (item);
        }

        public static T shift<T> (this List<T> list) {
            var result = list[0];
            list.RemoveAt (0);
            return result;
        }

        public static string slice (this string s, int startIndex) {
            return s.Substring (startIndex);
        }

        public static string slice (this string s, int startIndex, int endIndex) {
            return s.Substring (startIndex, endIndex - startIndex);
        }

        public static int codePointAt (this string s, int index) {
            return char.ConvertToUtf32 (s, index);
        }

        public static Match match (this string s, Regex regex) {
            return regex.Match (s);
        }

        public static T[] slice<T> (this T[] array, int begin, int end) {
            if (begin < 0)
                throw new ArgumentOutOfRangeException ("begin");
            else if (end < 0)
                throw new ArgumentOutOfRangeException ("end");

            if (end > array.Length)
                end = array.Length;

            int count = end - begin;
            var result = new T[count];
            for (int i = 0; i < count; i++)
                result[i] = array[begin + i];

            return result;
        }

        public static string join<T> (this IEnumerable<T> values, string separator) {
            return string.Join (separator, values);
        }

        public static double log1p (double v) {
            return Math.Log (v + 1);
        }
    }
}
