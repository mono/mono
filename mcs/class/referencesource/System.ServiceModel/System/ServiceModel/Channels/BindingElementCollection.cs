//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.ServiceModel;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    public class BindingElementCollection : Collection<BindingElement>
    {
        public BindingElementCollection()
        {
        }

        public BindingElementCollection(IEnumerable<BindingElement> elements)
        {
            if (elements == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elements");

            foreach (BindingElement element in elements)
            {
                base.Add(element);
            }
        }

        public BindingElementCollection(BindingElement[] elements)
        {
            if (elements == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elements");

            for (int i = 0; i < elements.Length; i++)
            {
                base.Add(elements[i]);
            }
        }

        internal BindingElementCollection(BindingElementCollection elements)
        {
            if (elements == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elements");

            for (int i = 0; i < elements.Count; i++)
            {
                base.Add(elements[i]);
            }
        }

        // returns a new collection with clones of all the elements
        public BindingElementCollection Clone()
        {
            BindingElementCollection result = new BindingElementCollection();
            for (int i = 0; i < this.Count; i++)
            {
                result.Add(this[i].Clone());
            }
            return result;
        }

        public void AddRange(params BindingElement[] elements)
        {
            if (elements == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elements");

            for (int i = 0; i < elements.Length; i++)
            {
                base.Add(elements[i]);
            }
        }

        public bool Contains(Type bindingElementType)
        {
            if (bindingElementType == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElementType");

            for (int i = 0; i < this.Count; i++)
            {
                if (bindingElementType.IsInstanceOfType(this[i]))
                    return true;
            }
            return false;
        }

        public T Find<T>()
        {
            return Find<T>(false);
        }

        public T Remove<T>()
        {
            return Find<T>(true);
        }

        T Find<T>(bool remove)
        {
            for (int index = 0; index < this.Count; index++)
            {
                if (this[index] is T)
                {
                    T item = (T)(object)this[index];
                    if (remove)
                    {
                        RemoveAt(index);
                    }
                    return item;
                }
            }
            return default(T);
        }

        public Collection<T> FindAll<T>()
        {
            return FindAll<T>(false);
        }

        public Collection<T> RemoveAll<T>()
        {
            return FindAll<T>(true);
        }

        Collection<T> FindAll<T>(bool remove)
        {
            Collection<T> collection = new Collection<T>();

            for (int index = 0; index < this.Count; index++)
            {
                if (this[index] is T)
                {
                    T item = (T)(object)this[index];
                    if (remove)
                    {
                        RemoveAt(index);
                        // back up the index so we inspect the new item at this location
                        index--;
                    }
                    collection.Add(item);
                }
            }

            return collection;
        }

        protected override void InsertItem(int index, BindingElement item)
        {
            if (item == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, BindingElement item)
        {
            if (item == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");

            base.SetItem(index, item);
        }
    }

}
