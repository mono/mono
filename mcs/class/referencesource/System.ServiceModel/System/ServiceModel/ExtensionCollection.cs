//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    public sealed class ExtensionCollection<T> : SynchronizedCollection<IExtension<T>>, IExtensionCollection<T>
        where T : IExtensibleObject<T>
    {
        T owner;

        public ExtensionCollection(T owner)
        {
            if (owner == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("owner");

            this.owner = owner;
        }

        public ExtensionCollection(T owner, object syncRoot)
            : base(syncRoot)
        {
            if (owner == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("owner");

            this.owner = owner;
        }

        bool ICollection<IExtension<T>>.IsReadOnly
        {
            get { return false; }
        }

        protected override void ClearItems()
        {
            IExtension<T>[] array;

            lock (this.SyncRoot)
            {
                array = new IExtension<T>[this.Count];
                this.CopyTo(array, 0);
                base.ClearItems();

                foreach (IExtension<T> extension in array)
                {
                    extension.Detach(this.owner);
                }
            }
        }

        public E Find<E>()
        {
            List<IExtension<T>> items = this.Items;

            lock (this.SyncRoot)
            {
                for (int i = this.Count - 1; i >= 0; i--)
                {
                    IExtension<T> item = items[i];
                    if (item is E)
                        return (E)item;
                }
            }

            return default(E);
        }

        public Collection<E> FindAll<E>()
        {
            Collection<E> result = new Collection<E>();
            List<IExtension<T>> items = this.Items;

            lock (this.SyncRoot)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    IExtension<T> item = items[i];
                    if (item is E)
                        result.Add((E)item);
                }
            }

            return result;
        }

        protected override void InsertItem(int index, IExtension<T> item)
        {
            if (item == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");

            lock (this.SyncRoot)
            {
                item.Attach(this.owner);
                base.InsertItem(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (this.SyncRoot)
            {
                this.Items[index].Detach(this.owner);
                base.RemoveItem(index);
            }
        }

        protected override void SetItem(int index, IExtension<T> item)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCannotSetExtensionsByIndex)));
        }
    }
}
