//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    sealed class BinaryHeap<TKey, TValue> where TKey : IComparable<TKey>
    {
        const int defaultCapacity = 128;
        readonly KeyValuePair<TKey, TValue> EmptyItem = new KeyValuePair<TKey, TValue>();
        KeyValuePair<TKey, TValue>[] items;
        int itemCount;

        public BinaryHeap() :
            this(defaultCapacity)
        {
        }

        public BinaryHeap(int capacity)
        {
            Fx.Assert(capacity > 0, "Capacity must be a positive value.");
            this.items = new KeyValuePair<TKey, TValue>[capacity];
        }

        public int Count
        {
            get { return this.itemCount; }
        }

        public bool IsEmpty
        {
            get { return this.itemCount == 0; }
        }

        public void Clear()
        {
            this.itemCount = 0;
            this.items = new KeyValuePair<TKey, TValue>[defaultCapacity];
        }

        public bool Enqueue(TKey key, TValue item)
        {
            if (this.itemCount == this.items.Length)
            {
                ResizeItemStore(this.items.Length * 2);
            }

            this.items[this.itemCount++] = new KeyValuePair<TKey, TValue>(key, item);
            int position = this.BubbleUp(this.itemCount - 1);

            return (position == 0);
        }

        public KeyValuePair<TKey, TValue> Dequeue()
        {
            return Dequeue(true);
        }

        KeyValuePair<TKey, TValue> Dequeue(bool shrink)
        {
            Fx.Assert(this.itemCount > 0, "Cannot dequeue empty queue.");

            KeyValuePair<TKey, TValue> result = items[0];

            if (this.itemCount == 1)
            {
                this.itemCount = 0;
                this.items[0] = this.EmptyItem;
            }
            else
            {
                --this.itemCount;
                this.items[0] = this.items[itemCount];
                this.items[itemCount] = this.EmptyItem;

                // Keep the structure of the heap valid.
                this.BubbleDown(0);
            }

            if (shrink)
            {
                ShrinkStore();
            }

            return result;
        }

        public KeyValuePair<TKey, TValue> Peek()
        {
            Fx.Assert(this.itemCount > 0, "Cannot peek at empty queue.");
            return this.items[0];
        }

        [SuppressMessage(FxCop.Category.Design, "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an internal only API.")]
        [SuppressMessage(FxCop.Category.MSInternal, "CA908:UseApprovedGenericsForPrecompiledAssemblies")]
        public ICollection<KeyValuePair<TKey, TValue>> RemoveAll(Predicate<KeyValuePair<TKey, TValue>> func)
        {
            ICollection<KeyValuePair<TKey, TValue>> result = new List<KeyValuePair<TKey, TValue>>();

            for (int position = 0; position < this.itemCount; position++)
            {
                while (func(this.items[position]) && position < this.itemCount)
                {
                    result.Add(this.items[position]);

                    int lastItem = this.itemCount - 1;

                    while (func(this.items[lastItem]) && position < lastItem)
                    {
                        result.Add(this.items[lastItem]);
                        this.items[lastItem] = EmptyItem;
                        --lastItem;
                    }

                    this.items[position] = this.items[lastItem];
                    this.items[lastItem] = EmptyItem;
                    this.itemCount = lastItem;

                    if (position < lastItem)
                    {
                        this.BubbleDown(this.BubbleUp(position));
                    }
                }
            }

            this.ShrinkStore();

            return result;
        }

        void ShrinkStore()
        {
            // If we are under half capacity and above default capacity size down.
            if (this.items.Length > defaultCapacity && this.itemCount < (this.items.Length >> 1))
            {
                int newSize = Math.Max(
                    defaultCapacity, (((this.itemCount / defaultCapacity) + 1) * defaultCapacity));

                this.ResizeItemStore(newSize);
            }
        }

        [SuppressMessage("Microsoft.MSInternal", "CA908:UseApprovedGenericsForPrecompiledAssemblies")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an internal only API.")]
        public ICollection<KeyValuePair<TKey, TValue>> TakeWhile(Predicate<TKey> func)
        {
            ICollection<KeyValuePair<TKey, TValue>> result = new List<KeyValuePair<TKey, TValue>>();

            while (!this.IsEmpty && func(this.Peek().Key))
            {
                result.Add(this.Dequeue(false));
            }

            ShrinkStore();

            return result;
        }

        void ResizeItemStore(int newSize)
        {
            Fx.Assert(itemCount < newSize, "Shrinking now will lose data.");
            Fx.Assert(defaultCapacity <= newSize, "Can not shrink below the default capacity.");

            KeyValuePair<TKey, TValue>[] temp = new KeyValuePair<TKey, TValue>[newSize];

            Array.Copy(this.items, 0, temp, 0, this.itemCount);

            this.items = temp;
        }

        void BubbleDown(int startIndex)
        {
            int currentPosition = startIndex;
            int swapPosition = startIndex;

            while (true)
            {
                int leftChildPosition = (currentPosition << 1) + 1;
                int rightChildPosition = leftChildPosition + 1;

                if (leftChildPosition < itemCount)
                {
                    if (this.items[currentPosition].Key.CompareTo(this.items[leftChildPosition].Key) > 0)
                    {
                        swapPosition = leftChildPosition;
                    }
                }
                else
                {
                    break;
                }

                if (rightChildPosition < itemCount)
                {
                    if (this.items[swapPosition].Key.CompareTo(this.items[rightChildPosition].Key) > 0)
                    {
                        swapPosition = rightChildPosition;
                    }
                }

                if (currentPosition != swapPosition)
                {
                    KeyValuePair<TKey, TValue> temp = this.items[currentPosition];
                    this.items[currentPosition] = this.items[swapPosition];
                    this.items[swapPosition] = temp;
                }
                else
                {
                    break;
                }

                currentPosition = swapPosition;
            }
        }

        int BubbleUp(int startIndex)
        {
            while (startIndex > 0)
            {
                int parent = (startIndex - 1) >> 1;

                if (this.items[parent].Key.CompareTo(this.items[startIndex].Key) > 0)
                {
                    KeyValuePair<TKey, TValue> temp = this.items[startIndex];
                    this.items[startIndex] = this.items[parent];
                    this.items[parent] = temp;
                }
                else
                {
                    break;
                }

                startIndex = parent;
            }

            return startIndex;
        }
    }
}
