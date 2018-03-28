//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Collections.Generic;
    using System.Collections;

    internal class CommunicationObjectManager<ItemType> : LifetimeManager where ItemType : class, ICommunicationObject
    {
        bool inputClosed;
        Hashtable table;

        public CommunicationObjectManager(object mutex)
            : base(mutex)
        {
            this.table = new Hashtable();
        }

        public void Add(ItemType item)
        {
            bool added = false;

            lock (this.ThisLock)
            {
                if (this.State == LifetimeState.Opened && !this.inputClosed)
                {
                    if (this.table.ContainsKey(item))
                        return;

                    this.table.Add(item, item);
                    base.IncrementBusyCountWithoutLock();
                    item.Closed += this.OnItemClosed;
                    added = true;
                }
            }

            if (!added)
            {
                item.Abort();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().ToString()));
            }
        }

        public void CloseInput()
        {
            //Abort can reenter this call as a result of 
            //close timeout, Closing input twice is not a
            //FailFast case.
            this.inputClosed = true;
        }

        public void DecrementActivityCount()
        {
            this.DecrementBusyCount();
        }

        public void IncrementActivityCount()
        {
            this.IncrementBusyCount();
        }

        void OnItemClosed(object sender, EventArgs args)
        {
            this.Remove((ItemType)sender);
        }

        public void Remove(ItemType item)
        {
            lock (this.ThisLock)
            {
                if (!this.table.ContainsKey(item))
                    return;
                this.table.Remove(item);
            }

            item.Closed -= this.OnItemClosed;
            base.DecrementBusyCount();
        }

        public ItemType[] ToArray()
        {
            lock (this.ThisLock)
            {
                int index = 0;
                ItemType[] items = new ItemType[this.table.Keys.Count];
                foreach (ItemType item in this.table.Keys)
                    items[index++] = item;

                return items;
            }
        }
    }
}
