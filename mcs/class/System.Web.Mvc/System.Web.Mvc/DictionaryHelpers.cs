/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class DictionaryHelpers {

        public static IEnumerable<KeyValuePair<string, TValue>> FindKeysWithPrefix<TValue>(IDictionary<string, TValue> dictionary, string prefix) {
            TValue exactMatchValue;
            if (dictionary.TryGetValue(prefix, out exactMatchValue)) {
                yield return new KeyValuePair<string, TValue>(prefix, exactMatchValue);
            }

            foreach (var entry in dictionary) {
                string key = entry.Key;

                if (key.Length <= prefix.Length) {
                    continue;
                }

                if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                char charAfterPrefix = key[prefix.Length];
                switch (charAfterPrefix) {
                    case '[':
                    case '.':
                        yield return entry;
                        break;
                }
            }
        }

        public static bool DoesAnyKeyHavePrefix<TValue>(IDictionary<string, TValue> dictionary, string prefix) {
            return FindKeysWithPrefix(dictionary, prefix).Any();
        }

    }
}
