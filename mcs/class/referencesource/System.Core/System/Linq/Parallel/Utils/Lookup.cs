// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// Lookup.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// Lookup class implements the ILookup interface. Lookup is very similar to a dictionary
    /// except multiple values are allowed to map to the same key, and null keys are supported.
    ///
    /// Support for null keys adds an issue because the Dictionary class Lookup uses for
    /// storage does not support null keys. So, we need to treat null keys separately.
    /// Unfortunately, since TKey may be a value type, we cannot test whether the key is null
    /// using the user-specified equality comparer.
    ///
    /// C# does allow us to compare the key against null using the == operator, but there is a
    /// possibility that the user's equality comparer considers null to be equal to other values.
    /// Now, MSDN documentation specifies that if IEqualityComparer.Equals(x,y) returns true, it
    /// must be the case that x and y have the same hash code, and null has no hash code. Despite
    /// that, we might as well support the use case, even if it is bad practice.
    ///
    /// The solution the Lookup class uses is to treat the key default(TKey) as a special case,
    /// and hold its associated grouping - if any - in a special field instead of inserting it
    /// into a dictionary.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    internal class Lookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        private IDictionary<TKey, IGrouping<TKey, TElement>> m_dict;
        private IEqualityComparer<TKey> m_comparer;
        private IGrouping<TKey, TElement> m_defaultKeyGrouping = null;

        internal Lookup(IEqualityComparer<TKey> comparer)
        {
            m_comparer = comparer;
            m_dict = new Dictionary<TKey, IGrouping<TKey, TElement>>(m_comparer);
        }

        public int Count
        {
            get
            {
                int count = m_dict.Count;
                if (m_defaultKeyGrouping != null)
                {
                    count++;
                }

                return count;
            }
        }

        // Returns an empty sequence if the key is not in the lookup.
        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                if (m_comparer.Equals(key, default(TKey)))
                {
                    if (m_defaultKeyGrouping != null)
                    {
                        return m_defaultKeyGrouping;
                    }

                    return Enumerable.Empty<TElement>();
                }
                else
                {
                    IGrouping<TKey, TElement> grouping;
                    if (m_dict.TryGetValue(key, out grouping))
                    {
                        return grouping;
                    }

                    return Enumerable.Empty<TElement>();
                }
            }
        }

        public bool Contains(TKey key)
        {
            if (m_comparer.Equals(key, default(TKey)))
            {
                return m_defaultKeyGrouping != null;
            }
            else
            {
                return m_dict.ContainsKey(key);
            }
        }

        //
        // Adds a grouping to the lookup
        //
        // Note: The grouping should be cheap to enumerate (IGrouping extends IEnumerable), as
        // it may be enumerated multiple times depending how the user manipulates the lookup.
        // Our code must guarantee that we never attempt to insert two groupings with the same
        // key into a lookup.
        //

        internal void Add(IGrouping<TKey, TElement> grouping)
        {
            if (m_comparer.Equals(grouping.Key, default(TKey)))
            {
                Contract.Assert(m_defaultKeyGrouping == null, "Cannot insert two groupings with the default key into a lookup.");

                m_defaultKeyGrouping = grouping;
            }
            else
            {
                Contract.Assert(!m_dict.ContainsKey(grouping.Key));

                m_dict.Add(grouping.Key, grouping);
            }
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            // First iterate over the groupings in the dictionary, and then over the default-key
            // grouping, if there is one.

            foreach (IGrouping<TKey, TElement> grouping in m_dict.Values)
            {
                yield return grouping;
            }

            if (m_defaultKeyGrouping != null)
            {
                yield return m_defaultKeyGrouping;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IGrouping<TKey, TElement>>)this).GetEnumerator();
        }
    }
}
