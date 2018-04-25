//------------------------------------------------------------------------------
// <copyright file="_PrefixLookup.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

//
// This internal class implements a data structure which can be
// used for storing a set of objects keyed by string prefixes
// Looking up an object given a string returns the value associated
// with the longest matching prefix
// (A prefix "matches" a string IFF the string starts with that prefix
// The degree of the match is prefix length)
//
// The class has a configurable maximum capacity.  When adding items, if the
// list is over capacity, then the least recently used (LRU) item is dropped.
//

namespace System.Net
{
    using System.Collections.Generic;
    using System.Diagnostics;

    internal class PrefixLookup
    {

        // Do not go over this limit.  Discard old data elements
        // Longer lists suffer a search penalty
        private const int defaultCapacity = 100;
        private volatile int capacity;

        // LRU list - Least Recently Used.  
        // Add new items to the front.  Drop items from the end if beyond capacity.
        // Promote used items to the top.
        private readonly LinkedList<PrefixValuePair> lruList = new LinkedList<PrefixValuePair>();

        private class PrefixValuePair
        {
            public string prefix;
            public object value;

            public PrefixValuePair(string pre, object val)
            {
                prefix = pre;
                value = val;
            }
        }

        public PrefixLookup() : this(defaultCapacity)
        {
        }

        public PrefixLookup(int capacity)
        {
            this.capacity = capacity;
        }

#if DEBUG
        // this method is only called by test code
        internal int Capacity
        {
            get { return capacity; }
            set
            {
                lock (lruList)
                {
                    if (value <= 0)
                    {
                        // Disabled, flush list
                        capacity = 0;
                        lruList.Clear();
                    }
                    else
                    {
                        capacity = value;

                        // Ensure list is still within capacity
                        while (lruList.Count > capacity)
                        {
                            lruList.RemoveLast();
                        }
                    }
                }
            }
        }
#endif

        public void Add(string prefix, object value)
        {
            Debug.Assert(prefix != null, "PrefixLookup.Add; prefix must not be null");
            Debug.Assert(prefix.Length > 0, "PrefixLookup.Add; prefix must not be empty");
            Debug.Assert(value != null, "PrefixLookup.Add; value must not be null");

            if (capacity == 0 || prefix == null || prefix.Length == 0 || value == null)
                return;

            // writers are locked
            lock (lruList)
            {
                // Special case duplicate check at start of list, very common
                if (lruList.First != null && lruList.First.Value.prefix.Equals(prefix))
                {
                    // Already in list, update value
                    lruList.First.Value.value = value;
                }
                else
                {
                    // New entry
                    // Duplicates will just be pushed down and eventually discarded
                    lruList.AddFirst(new PrefixValuePair(prefix, value));

                    // If full, drop the least recently used
                    while (lruList.Count > capacity)
                    {
                        lruList.RemoveLast();
                    }
                }

            }
        }

        public object Lookup(string lookupKey)
        {
            Debug.Assert(lookupKey != null, "PrefixLookup.Lookup; lookupKey must not be null");
            Debug.Assert(lookupKey.Length > 0, "PrefixLookup.Lookup; lookupKey must not be empty");

            if (lookupKey == null || lookupKey.Length == 0 || lruList.Count == 0)
            {
                return null;
            }

            LinkedListNode<PrefixValuePair> mostSpecificMatch = null;
            lock (lruList)
            {
                //
                // Normally readers don't need to be locked, but if the value is found
                // then it is promoted to the top of the list.
                //

                // Oh well, do it the slow way, search for the longest partial match
                string prefix;
                int longestMatchPrefix = 0;
                for (LinkedListNode<PrefixValuePair> pairNode = lruList.First;
                    pairNode != null; pairNode = pairNode.Next)
                {
                    //
                    // check if the match is better than the current-most-specific match
                    //
                    prefix = pairNode.Value.prefix;
                    if (prefix.Length > longestMatchPrefix && lookupKey.StartsWith(prefix))
                    {
                        //
                        // Yes-- update the information about currently preferred match
                        //
                        longestMatchPrefix = prefix.Length;
                        mostSpecificMatch = pairNode;

                        if (longestMatchPrefix == lookupKey.Length)
                            break; // Exact match, optimal solution.
                    }
                }

                if (mostSpecificMatch != null && mostSpecificMatch != lruList.First)
                {
                    // We have a match and it's not the first element, move it up in the list
                    lruList.Remove(mostSpecificMatch);
                    lruList.AddFirst(mostSpecificMatch);
                }
            }
            return mostSpecificMatch != null ? mostSpecificMatch.Value.value : null;
        }
    } // class PrefixLookup
}
