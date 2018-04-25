//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Runtime
{
    using System.Collections.Generic;

    class DuplicateDetector<T>
        where T : class
    {
        LinkedList<T> fifoList;
        Dictionary<T, LinkedListNode<T>> items;
        int capacity;
        object thisLock;

        public DuplicateDetector(int capacity)
        {
            Fx.Assert(capacity >= 0, "The capacity parameter must be a positive value.");

            this.capacity = capacity;
            this.items = new Dictionary<T, LinkedListNode<T>>();
            this.fifoList = new LinkedList<T>();
            this.thisLock = new object();
        }

        public bool AddIfNotDuplicate(T value)
        {
            Fx.Assert(value != null, "The value must be non null.");
            bool success = false;

            lock (this.thisLock)
            {
                if (!this.items.ContainsKey(value))
                {
                    Add(value);
                    success = true;
                }
            }

            return success;
        }

        void Add(T value)
        {
            Fx.Assert(this.items.Count == this.fifoList.Count, "The items and fifoList must be synchronized.");

            if (this.items.Count == this.capacity)
            {
                LinkedListNode<T> node = this.fifoList.Last;
                this.items.Remove(node.Value);
                this.fifoList.Remove(node);
            }

            this.items.Add(value, this.fifoList.AddFirst(value));
        }

        public bool Remove(T value)
        {
            Fx.Assert(value != null, "The value must be non null.");

            bool success = false;
            LinkedListNode<T> node;
            lock (this.thisLock)
            {
                if (this.items.TryGetValue(value, out node))
                {
                    this.items.Remove(value);
                    this.fifoList.Remove(node);
                    success = true;
                }
            }

            return success;
        }

        public void Clear()
        {
            lock (this.thisLock)
            {
                this.fifoList.Clear();
                this.items.Clear();
            }
        }
    }
}
