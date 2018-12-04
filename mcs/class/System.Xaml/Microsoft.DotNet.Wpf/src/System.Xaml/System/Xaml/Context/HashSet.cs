using System;
using System.Collections.Generic;

namespace System.Xaml.Context
{
    internal class HashSet<T> : Dictionary<T, object>
    {
        public HashSet()
            : base()
        {
        }

        public HashSet(IDictionary<T, object> other)
            : base(other)
        {
        }

        public HashSet(IEqualityComparer<T> comparer)
            : base(comparer)
        {
        }

        public void Add(T item)
        {
            Add(item, null);
        }
    }
}
