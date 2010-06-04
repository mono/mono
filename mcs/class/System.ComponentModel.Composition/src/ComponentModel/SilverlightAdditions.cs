// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System
{
    [Conditional("NOT_SILVERLIGHT")]    // Trick so that the attribute is never actually applied
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate, Inherited = false)]
    internal sealed class SerializableAttribute : Attribute
    {
    }
}

namespace System.ComponentModel
{
    internal sealed class LocalizableAttribute : Attribute
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "isLocalizable")]
        public LocalizableAttribute(bool isLocalizable)
        {
        }
    }
}

#if !CLR40
namespace System.Collections.Generic
{
    internal class HashSet<T> : IEnumerable<T>
    {
        private Dictionary<T, object> _set = new Dictionary<T, object>();

        public HashSet()
        {
        }

        public HashSet(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Add(item);
            }
        }

        public bool Add(T item)
        {
            if (!this._set.ContainsKey(item))
            {
                this._set.Add(item, null);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            this._set.Clear();
        }

        public bool Contains(T item)
        {
            return this._set.ContainsKey(item);
        }

        public bool Remove(T item)
        {
            if (this._set.ContainsKey(item))
            {
                this._set.Remove(item);
                return true;
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this._set.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
#endif
