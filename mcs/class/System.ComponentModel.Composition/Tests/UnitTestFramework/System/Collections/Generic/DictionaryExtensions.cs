// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Microsoft.Internal.Collections
{
    public static class DictionaryExtensions
    {
        public static bool ContainsAllKeys<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
        {
            foreach (TKey key in keys)
            {
                if (!dictionary.ContainsKey(key))
                    return false;
            }

            return true;
        }

        public static bool DictionaryEquals<TKey, TValue>(this IDictionary<TKey, TValue> dictionary1, IDictionary<TKey, TValue> dictionary2)
        {
            if (dictionary1.Keys.Count != dictionary2.Keys.Count)
            {
                return false;
            }

            foreach (KeyValuePair<TKey, TValue> kvp in dictionary1)
            {
                TValue value1 = kvp.Value;
                TValue value2 = default(TValue);
                if (!dictionary2.TryGetValue(kvp.Key, out value2))
                {
                    return false;
                }

                IDictionary<TKey, TValue> nestedDictionary1 = value1 as IDictionary<TKey, TValue>;
                IDictionary<TKey, TValue> nestedDictionary2 = value1 as IDictionary<TKey, TValue>;

                if ((nestedDictionary1 != null) && (nestedDictionary2 != null))
                {
                    if (!nestedDictionary1.DictionaryEquals(nestedDictionary2))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!(value1.Equals(value2)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}