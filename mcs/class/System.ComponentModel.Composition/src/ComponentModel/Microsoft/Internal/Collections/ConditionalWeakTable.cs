// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
#if !CLR40
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Internal;
using Microsoft.Internal.Collections;

namespace Microsoft.Internal.Collections
{
    // This is a broken implementation of ConditionalWeakTable that allows us
    // to compile and work on versions of .Net eariler then 4.0. This class is
    // broken when there are circular dependencies between keys and values, which
    // can only be fixed by using some specific CLR 4.0 features.
    // For code samples of the broken behavior see ConditionalWeakTableTests.cs.
    internal class ConditionalWeakTable<TKey, TValue> 
        where TKey : class
        where TValue : class
    {
        private readonly Dictionary<object, TValue> _table;
        private int _capacity = 4;

        public ConditionalWeakTable()
        {
            this._table = new Dictionary<object, TValue>();
        }

        public void Add(TKey key, TValue value)
        {
            CleanupDeadReferences();
            this._table.Add(CreateWeakKey(key), value);
        }

        public bool Remove(TKey key)
        {
            return this._table.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this._table.TryGetValue(key, out value);
        }

        private void CleanupDeadReferences()
        {
            if (this._table.Count < _capacity)
            {
                return;
            }

            object[] deadKeys = this._table.Keys
                .Where(weakRef => !((EquivalentWeakReference)weakRef).IsAlive).ToArray();

            foreach (var deadKey in deadKeys)
            {
                this._table.Remove(deadKey);
            }

            if (this._table.Count >= _capacity)
            {
                _capacity *= 2;
            }
        }

        private static object CreateWeakKey(TKey key)
        {
            return new EquivalentWeakReference(key);
        }

        private class EquivalentWeakReference
        {
            private readonly WeakReference _weakReference;
            private readonly int _hashCode;

            public EquivalentWeakReference(object obj)
            {
                this._hashCode = obj.GetHashCode();
                this._weakReference = new WeakReference(obj);
            }

            public bool IsAlive
            {
                get
                {
                    return this._weakReference.IsAlive;
                }
            }

            public override bool Equals(object obj)
            {
                EquivalentWeakReference weakRef = obj as EquivalentWeakReference;

                if (weakRef != null)
                {
                    obj = weakRef._weakReference.Target;
                }

                if (obj == null)
                {
                    return base.Equals(weakRef);
                }
                
                return object.Equals(this._weakReference.Target, obj);
            }

            public override int GetHashCode()
            {
                return this._hashCode;
            }
        }
    }
}
#endif