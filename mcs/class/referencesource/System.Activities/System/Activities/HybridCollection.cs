//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Collections.ObjectModel;

    // used internally for performance in cases where a common usage pattern is a single item
    [DataContract]
    class HybridCollection<T>
        where T : class
    {
        List<T> multipleItems;
        T singleItem;

        public HybridCollection()
        {
        }

        public HybridCollection(T initialItem)
        {
            Fx.Assert(initialItem != null, "null is used as a sentinal value and is not a valid item value for a hybrid collection");
            this.singleItem = initialItem;
        }

        public T this[int index]
        {
            get
            {
                if (this.singleItem != null)
                {
                    Fx.Assert(index == 0, "Out of range with a single item");
                    return this.singleItem;
                }
                else if (this.multipleItems != null)
                {
                    Fx.Assert(index >= 0 && index < this.multipleItems.Count, "Out of range with multiple items.");

                    return this.multipleItems[index];
                }

                Fx.Assert("Out of range.  There were no items in the HybridCollection.");
                return default(T);
            }
        }

        public int Count
        {
            get
            {
                if (this.singleItem != null)
                {
                    return 1;
                }

                if (this.multipleItems != null)
                {
                    return this.multipleItems.Count;
                }

                return 0;
            }
        }

        protected T SingleItem
        {
            get
            {
                return this.singleItem;
            }
        }

        protected IList<T> MultipleItems
        {
            get
            {
                return this.multipleItems;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "multipleItems")]
        internal List<T> SerializedMultipleItems
        {
            get { return this.multipleItems; }
            set { this.multipleItems = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "singleItem")]
        internal T SerializedSingleItem
        {
            get { return this.singleItem; }
            set { this.singleItem = value; }
        }

        public void Add(T item)
        {
            Fx.Assert(item != null, "null is used as a sentinal value and is not a valid item value for a hybrid collection");
            if (this.multipleItems != null)
            {
                this.multipleItems.Add(item);
            }
            else if (this.singleItem != null)
            {
                this.multipleItems = new List<T>(2);
                this.multipleItems.Add(this.singleItem);
                this.multipleItems.Add(item);
                this.singleItem = null;
            }
            else
            {
                this.singleItem = item;
            }
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            if (this.multipleItems != null)
            {
                return new ReadOnlyCollection<T>(this.multipleItems);
            }
            else if (this.singleItem != null)
            {
                return new ReadOnlyCollection<T>(new T[1] { this.singleItem });
            }
            else
            {
                return new ReadOnlyCollection<T>(new T[0]);
            }
        }

        // generally used for serialization purposes
        public void Compress()
        {
            if (this.multipleItems != null && this.multipleItems.Count == 1)
            {
                this.singleItem = this.multipleItems[0];
                this.multipleItems = null;
            }
        }

        public void Remove(T item)
        {
            Remove(item, false);
        }

        internal void Remove(T item, bool searchingFromEnd)
        {
            if (this.singleItem != null)
            {
                Fx.Assert(object.Equals(item, this.singleItem), "The given item should be in this list. Something is wrong in our housekeeping.");
                this.singleItem = null;
            }
            else
            {
                Fx.Assert(this.multipleItems != null && this.multipleItems.Contains(item), "The given item should be in this list. Something is wrong in our housekeeping.");
                int position = (searchingFromEnd) ? this.multipleItems.LastIndexOf(item) : this.multipleItems.IndexOf(item);
                if (position != -1)
                {
                    this.multipleItems.RemoveAt(position);
                }
            }
        }
    }
}
