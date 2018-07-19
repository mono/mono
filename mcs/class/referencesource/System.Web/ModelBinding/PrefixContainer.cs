namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// This is a container for prefix values. It normalizes all the values into dotted-form and then stores
    /// them in a sorted array. All queries for prefixes are also normalized to dotted-form, and searches
    /// for ContainsPrefix are done with a binary search.
    /// </summary>
    internal sealed class PrefixContainer {

        private readonly string[] _sortedValues;

        internal PrefixContainer(IEnumerable<string> values) {
            if (values == null) {
                throw new ArgumentNullException("values");
            }

            _sortedValues = values.Where(val => val != null).ToArray();
            Array.Sort(_sortedValues, StringComparer.OrdinalIgnoreCase);
        }

        internal bool ContainsPrefix(string prefix) {
            if (prefix == null) {
                throw new ArgumentNullException("prefix");
            }

            if (prefix.Length == 0) {
                return _sortedValues.Length > 0; // only match empty string when we have some value
            }

            return Array.BinarySearch(_sortedValues, prefix, new PrefixComparer(prefix)) > -1;
        }

        internal static bool IsPrefixMatch(string prefix, string testString) {
            if (testString == null) {
                return false;
            }

            if (prefix.Length == 0) {
                return true; // shortcut - non-null testString matches empty prefix
            }

            if (prefix.Length > testString.Length) {
                return false; // not long enough
            }

            if (!testString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                return false; // prefix doesn't match
            }

            if (testString.Length == prefix.Length) {
                return true; // exact match
            }

            // invariant: testString.Length > prefix.Length
            switch (testString[prefix.Length]) {
                case '.':
                case '[':
                    return true; // known delimiters

                default:
                    return false; // not known delimiter
            }
        }

        private sealed class PrefixComparer : IComparer<String> {
            private string _prefix;

            public PrefixComparer(string prefix) {
                _prefix = prefix;
            }

            public int Compare(string x, string y) {
                string testString = Object.ReferenceEquals(x, _prefix) ? y : x;
                if (IsPrefixMatch(_prefix, testString)) {
                    return 0;
                }

                return StringComparer.OrdinalIgnoreCase.Compare(x, y);
            }
        }

    }
}
