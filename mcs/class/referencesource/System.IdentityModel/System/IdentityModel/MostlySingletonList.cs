//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Collections.Generic;

    // Embed this struct in a class to represent a field of that class
    // that is logically a list, but contains just one item in all but
    // the rarest of scenarios.  When this class must be passed around
    // in internal APIs, use it as a ref parameter.
    struct MostlySingletonList<T> where T : class
    {
        int count;
        T singleton;
        List<T> list;

        public T this[int index]
        {
            get
            {
                if (this.list == null)
                {
                    EnsureValidSingletonIndex(index);
                    return this.singleton;
                }
                else
                {
                    return this.list[index];
                }
            }
        }

        public int Count
        {
            get { return this.count; }
        }

        public void Add(T item)
        {
            if (this.list == null)
            {
                if (this.count == 0)
                {
                    this.singleton = item;
                    this.count = 1;
                    return;
                }
                this.list = new List<T>();
                this.list.Add(this.singleton);
                this.singleton = null;
            }
            this.list.Add(item);
            this.count++;
        }

        static bool Compare(T x, T y)
        {
            return x == null ? y == null : x.Equals(y);
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        void EnsureValidSingletonIndex(int index)
        {
            if (this.count != 1 )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeOne)));
            }

            if (index != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", SR.GetString(SR.ValueMustBeZero)));
            }

        }

        bool MatchesSingleton(T item)
        {
            return this.count == 1 && Compare(this.singleton, item);
        }

        public int IndexOf(T item)
        {
            if (this.list == null)
            {
                return MatchesSingleton(item) ? 0 : -1;
            }
            else
            {
                return this.list.IndexOf(item);
            }
        }

        public bool Remove(T item)
        {
            if (this.list == null)
            {
                if (MatchesSingleton(item))
                {
                    this.singleton = null;
                    this.count = 0;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                bool result = this.list.Remove(item);
                if (result)
                {
                    this.count--;
                }
                return result;
            }
        }

        public void RemoveAt(int index)
        {
            if (this.list == null)
            {
                EnsureValidSingletonIndex(index);
                this.singleton = null;
                this.count = 0;
            }
            else
            {
                this.list.RemoveAt(index);
                this.count--;
            }
        }
    }
}
