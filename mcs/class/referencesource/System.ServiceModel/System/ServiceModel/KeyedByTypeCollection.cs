//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Collections.Generic
{
    using System.Xml;
    using System.ServiceModel;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    public class KeyedByTypeCollection<TItem> : KeyedCollection<Type, TItem>
    {
        public KeyedByTypeCollection()
            : base(null, 4)
        {
        }

        public KeyedByTypeCollection(IEnumerable<TItem> items)
            : base(null, 4)
        {
            if (items == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("items");

            foreach (TItem item in items)
            {
                base.Add(item);
            }
        }

        public T Find<T>()
        {
            return this.Find<T>(false);
        }

        public T Remove<T>()
        {
            return this.Find<T>(true);
        }

        T Find<T>(bool remove)
        {
            for (int i = 0; i < this.Count; i++)
            {
                TItem settings = this[i];
                if (settings is T)
                {
                    if (remove)
                    {
                        Remove(settings);
                    }
                    return (T)(object)settings;
                }
            }
            return default(T);
        }

        public Collection<T> FindAll<T>()
        {
            return this.FindAll<T>(false);
        }

        public Collection<T> RemoveAll<T>()
        {
            return this.FindAll<T>(true);
        }

        Collection<T> FindAll<T>(bool remove)
        {
            Collection<T> result = new Collection<T>();
            foreach (TItem settings in this)
            {
                if (settings is T)
                {
                    result.Add((T)(object)settings);
                }
            }

            if (remove)
            {
                foreach (T settings in result)
                {
                    this.Remove((TItem)(object)settings);
                }
            }

            return result;
        }

        protected override Type GetKeyForItem(TItem item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }

            return item.GetType();
        }

        protected override void InsertItem(int index, TItem item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }

            if (this.Contains(item.GetType()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("item", SR.GetString(SR.DuplicateBehavior1, item.GetType().FullName));
            }

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, TItem item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }

            base.SetItem(index, item);
        }
    }
}
