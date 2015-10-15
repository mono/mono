//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;

#if NO
    internal interface IQueryBufferPool
    {
        // Clear all pools
        void Reset();
        // Trim pools
        void Trim();
    } 
#endif

    //
    // Generic struct representing ranges within buffers
    //
    internal struct QueryRange
    {
        internal int end;       // INCLUSIVE - the end of the range
        internal int start;     // INCLUSIVE - the start of the range
#if NO
        internal QueryRange(int offset, QueryRange range)
        {
            this.start = range.start + offset;
            this.end = range.end + offset;
        }
#endif
        internal QueryRange(int start, int end)
        {
            this.start = start;
            this.end = end;
        }

        internal int Count
        {
            get
            {
                return this.end - this.start + 1;
            }
        }
#if NO        
        internal int this[int offset]
        {
            get
            {
                return this.start + offset;
            }
        }
                
        internal bool IsNotEmpty
        {
            get
            {
                return (this.end >= this.start);
            }
        }

        internal void Clear()
        {
            this.end = this.start - 1;
        }
                        
        internal void Grow(int offset)
        {
            this.end += offset;
        }
#endif
        internal bool IsInRange(int point)
        {
            return (this.start <= point && point <= this.end);
        }
#if NO
        internal void Set(int start, int end)
        {
            this.start = start;
            this.end = end;
        }
#endif
        internal void Shift(int offset)
        {
            this.start += offset;
            this.end += offset;
        }
    }

    /// <summary>
    /// Our own buffer management
    /// There are a few reasons why we don't reuse something in System.Collections.Generic
    ///  1. We want Clear() to NOT reallocate the internal array. We want it to simply set the Count = 0
    ///     This allows us to reuse buffers with impunity.
    ///  2. We want to be able to replace the internal buffer in a collection with a different one. Again,
    ///     this is to help with pooling
    ///  3. We want to be able to control how fast buffers grow. 
    ///  4. Does absolutely no bounds or null checking. As fast as we can make it. All checking should be done
    ///  by whoever wraps this. Checking is unnecessary for many internal uses where we need optimal perf.
    ///  5. Does more precise trimming
    ///  6. AND this is a struct
    ///
    /// </summary>        
    internal struct QueryBuffer<T>
    {
        internal T[] buffer;    // buffer of T. Frequently larger than count
        internal int count;     // Actual # of items
        internal static T[] EmptyBuffer = new T[0];

        /// <summary>
        /// Construct a new buffer
        /// </summary>
        /// <param name="capacity"></param>
        internal QueryBuffer(int capacity)
        {
            if (0 == capacity)
            {
                this.buffer = QueryBuffer<T>.EmptyBuffer;
            }
            else
            {
                this.buffer = new T[capacity];
            }
            this.count = 0;
        }
#if NO
		internal QueryBuffer(QueryBuffer<T> buffer)
		{
		    this.buffer = (T[]) buffer.buffer.Clone();
		    this.count = buffer.count;
		}
		
		internal QueryBuffer(T[] buffer)
		{
      Fx.Assert(null != buffer, "");
            this.buffer = buffer;
            this.count = 0;
        }
	
		/// <summary>
		/// Get and set the internal buffer
		/// If you set the buffer, the count will automatically be set to 0
		/// </summary>
		internal T[] Buffer
		{
		    get
		    {
		        return this.buffer;
		    }
		    set
		    {
          Fx.Assert(null != value, "");
		        this.buffer = value;
		        this.count = 0;
		    }
		}
#endif
        /// <summary>
        /// # of items
        /// </summary>
        internal int Count
        {
            get
            {
                return this.count;
            }
#if NO
		    set
		    {
          Fx.Assert(value >= 0 && value <= this.buffer.Length, "");
		        this.count = value;
		    }
#endif
        }

#if NO
		/// <summary>
		/// How much can it hold
		/// </summary>
		internal int Capacity
		{
		    get
		    {
		        return this.buffer.Length;
		    }
		    set
		    {
          Fx.Assert(value >= this.count, "");
		        if (value > this.buffer.Length)
		        {
		            Array.Resize<T>(ref this.buffer, value);
		        }
		    }
		}
#endif

        internal T this[int index]
        {
            get
            {
                return this.buffer[index];
            }
            set
            {
                this.buffer[index] = value;
            }
        }

#if NO
		internal void Add()
		{
            if (this.count == this.buffer.Length)
            {
                Array.Resize<T>(ref this.buffer, this.count > 0 ? this.count * 2 : 16);
            }
            this.count++;
        }
#endif

        /// <summary>
        /// Add an element to the buffer
        /// </summary>
        internal void Add(T t)
        {
            if (this.count == this.buffer.Length)
            {
                Array.Resize<T>(ref this.buffer, this.count > 0 ? this.count * 2 : 16);
            }
            this.buffer[this.count++] = t;
        }

#if NO
        /// <summary>
        /// Useful when this is a buffer of structs
        /// </summary>
        internal void AddReference(ref T t)
        {
		    if (this.count == this.buffer.Length)
		    {
                Array.Resize<T>(ref this.buffer, this.count > 0 ? this.count * 2 : 16);
            }
            this.buffer[this.count++] = t;
        }
#endif

        /// <summary>
        /// Add all the elements in the given buffer to this one
        /// We can do this very efficiently using an Array Copy
        /// </summary>
        internal void Add(ref QueryBuffer<T> addBuffer)
        {
            if (1 == addBuffer.count)
            {
                this.Add(addBuffer.buffer[0]);
                return;
            }

            int newCount = this.count + addBuffer.count;
            if (newCount >= this.buffer.Length)
            {
                this.Grow(newCount);
            }
            // Copy all the new elements in
            Array.Copy(addBuffer.buffer, 0, this.buffer, this.count, addBuffer.count);
            this.count = newCount;
        }

#if NO 
        internal void Add(T[] addBuffer, int startAt, int addCount)
        {
            int newCount = this.count + addCount;
            if (newCount >= this.buffer.Length)
            {
                this.Grow(newCount);
            }
            // Copy all the new elements in
            Array.Copy(addBuffer, startAt, this.buffer, this.count, addCount);
            this.count = newCount;
        }
        
        /// <summary>
		/// Add without attempting to grow the buffer. Faster, but must be used with care.
		/// Caller must ensure that the buffer is large enough.
		/// </summary>
        internal void AddOnly(T t)
		{
		    this.buffer[this.count++] = t;
		}
#endif

        /// <summary>
        /// Set the count to zero but do NOT get rid of the actual buffer
        /// </summary>
        internal void Clear()
        {
            this.count = 0;
        }

#if NO        
        //
        // Copy from one location in the buffer to another
        //
        internal void Copy(int from, int to)
        {
            this.buffer[to] = this.buffer[from];    
        }
        
        internal void Copy(int from, int to, int count)
        {
            Array.Copy(this.buffer, from, this.buffer, to, count);
        }
#endif

        internal void CopyFrom(ref QueryBuffer<T> addBuffer)
        {
            int addCount = addBuffer.count;
            switch (addCount)
            {
                default:
                    if (addCount > this.buffer.Length)
                    {
                        this.buffer = new T[addCount];
                    }
                    // Copy all the new elements in
                    Array.Copy(addBuffer.buffer, 0, this.buffer, 0, addCount);
                    this.count = addCount;
                    break;

                case 0:
                    this.count = 0;
                    break;

                case 1:
                    if (this.buffer.Length == 0)
                    {
                        this.buffer = new T[1];
                    }
                    this.buffer[0] = addBuffer.buffer[0];
                    this.count = 1;
                    break;
            }
        }

        internal void CopyTo(T[] dest)
        {
            Array.Copy(this.buffer, dest, this.count);
        }

#if NO
        /// <summary>
		/// Ensure that the internal buffer has adequate capacity
		/// </summary>
		internal void EnsureCapacity(int capacity)
		{
		    if (capacity > this.buffer.Length)
		    {
		        this.Grow(capacity);
            }
		}		

  
        internal void Erase()
        {
            Array.Clear(this.buffer, 0, this.count);
            this.count = 0;
        }
#endif
        void Grow(int capacity)
        {
            int newCapacity = this.buffer.Length * 2;
            Array.Resize<T>(ref this.buffer, capacity > newCapacity ? capacity : newCapacity);
        }

        internal int IndexOf(T t)
        {
            for (int i = 0; i < this.count; ++i)
            {
                if (t.Equals(this.buffer[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        internal int IndexOf(T t, int startAt)
        {
            for (int i = startAt; i < this.count; ++i)
            {
                if (t.Equals(this.buffer[i]))
                {
                    return i;
                }
            }
            return -1;
        }
#if NO 
        internal void InsertAt(T t, int at)
        {
            this.ReserveAt(at, 1);
            this.buffer[at] = t;
        }
#endif
        internal bool IsValidIndex(int index)
        {
            return (index >= 0 && index < this.count);
        }
#if NO        
        internal T Pop()
        {
            Fx.Assert(this.count > 0, "");
            return this.buffer[--this.count];
        }
        
        internal void Push(T t)
        {
            this.Add(t);
        }
#endif

        /// <summary>
        /// Reserve enough space for count elements
        /// </summary>
        internal void Reserve(int reserveCount)
        {
            int newCount = this.count + reserveCount;
            if (newCount >= this.buffer.Length)
            {
                this.Grow(newCount);
            }
            this.count = newCount;
        }

        internal void ReserveAt(int index, int reserveCount)
        {
            if (index == this.count)
            {
                this.Reserve(reserveCount);
                return;
            }

            int newCount;
            if (index > this.count)
            {
                // We want to reserve starting at a location past what is current committed. 
                // No shifting needed
                newCount = index + reserveCount + 1;
                if (newCount >= this.buffer.Length)
                {
                    this.Grow(newCount);
                }
            }
            else
            {
                // reserving space within an already allocated portion of the buffer
                // we'll ensure that the buffer can fit 'newCount' items, then shift by reserveCount starting at index
                newCount = this.count + reserveCount;
                if (newCount >= this.buffer.Length)
                {
                    this.Grow(newCount);
                }
                // Move to make room
                Array.Copy(this.buffer, index, this.buffer, index + reserveCount, this.count - index);
            }
            this.count = newCount;
        }

        internal void Remove(T t)
        {
            int index = this.IndexOf(t);
            if (index >= 0)
            {
                this.RemoveAt(index);
            }
        }

        internal void RemoveAt(int index)
        {
            if (index < this.count - 1)
            {
                Array.Copy(this.buffer, index + 1, this.buffer, index, this.count - index - 1);
            }
            this.count--;
        }

        internal void Sort(IComparer<T> comparer)
        {
            Array.Sort<T>(this.buffer, 0, this.count, comparer);
        }
#if NO

        /// <summary>
        /// Reduce the buffer capacity so that it is no greater than twice the element count
        /// </summary>
        internal void Trim()
        {
            int maxSize = this.count * 2;
            if (maxSize < this.buffer.Length / 2)
            {
                if (0 == maxSize)
                {
                    this.buffer = QueryBuffer<T>.EmptyBuffer;
                }
                else
                {
                    T[] newBuffer = new T[maxSize];
                    Array.Copy(this.buffer, newBuffer, maxSize);
                }
            }
        }
#endif

        /// <summary>
        /// Reduce the buffer capacity so that its size is exactly == to the element count
        /// </summary>
        internal void TrimToCount()
        {
            if (this.count < this.buffer.Length)
            {
                if (0 == this.count)
                {
                    this.buffer = QueryBuffer<T>.EmptyBuffer;
                }
                else
                {
                    T[] newBuffer = new T[this.count];
                    Array.Copy(this.buffer, newBuffer, this.count);
                }
            }
        }
    }

    internal struct SortedBuffer<T, C>
            where C : IComparer<T>
    {
        int size;
        T[] buffer;
        static DefaultComparer Comparer;

        internal SortedBuffer(C comparerInstance)
        {
            this.size = 0;
            this.buffer = null;

            if (Comparer == null)
            {
                Comparer = new DefaultComparer(comparerInstance);
            }
            else
            {
                Fx.Assert(object.ReferenceEquals(DefaultComparer.Comparer, comparerInstance), "The SortedBuffer type has already been initialized with a different comparer instance.");
            }
        }

        internal T this[int index]
        {
            get
            {
                return GetAt(index);
            }
        }

        internal int Capacity
        {
#if NO        
            get
            {
                return this.buffer == null ? 0 : this.buffer.Length;
            }
#endif
            set
            {
                if (this.buffer != null)
                {
                    if (value != this.buffer.Length)
                    {
                        Fx.Assert(value >= this.size, "New capacity must be >= size");
                        if (value > 0)
                        {
                            Array.Resize(ref this.buffer, value);
                        }
                        else
                        {
                            this.buffer = null;
                        }
                    }
                }
                else
                {
                    this.buffer = new T[value];
                }
            }
        }

        internal int Count
        {
            get
            {
                return this.size;
            }
        }

        internal int Add(T item)
        {
            int i = Search(item);

            if (i < 0)
            {
                i = ~i;
                InsertAt(i, item);
            }

            return i;
        }

#if NO
        internal void CopyTo(T[] array)
        {
            CopyTo(array, 0, this.size);
        }

        internal void CopyTo(T[] array, int start, int length)
        {
            Fx.Assert(array != null, "");
            Fx.Assert(start >= 0, "");
            Fx.Assert(length >= 0, "");
            Fx.Assert(start + length < this.size, "");
            Array.Copy(this.buffer, 0, array, start, length);
        }
#endif
        internal void Clear()
        {
            this.size = 0;
        }
#if NO
        internal bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }
#endif
        internal void Exchange(T old, T replace)
        {
            if (Comparer.Compare(old, replace) == 0)
            {
                int i = IndexOf(old);
                if (i >= 0)
                {
                    this.buffer[i] = replace;
                }
                else
                {
                    Insert(replace);
                }
            }
            else
            {
                // PERF, Microsoft, can this be made more efficient?  Does it need to be?
                Remove(old);
                Insert(replace);
            }
        }

        internal T GetAt(int index)
        {
            Fx.Assert(index < this.size, "Index is greater than size");
            return this.buffer[index];
        }

        internal int IndexOf(T item)
        {
            return Search(item);
        }

        internal int IndexOfKey<K>(K key, IItemComparer<K, T> itemComp)
        {
            return Search(key, itemComp);
        }

        internal int Insert(T item)
        {
            int i = Search(item);

            if (i >= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new ArgumentException(SR.GetString(SR.QueryItemAlreadyExists)));
            }

            // If an item is not found, Search returns the bitwise negation of
            // the index an item should inserted at;
            InsertAt(~i, item);
            return ~i;
        }

        void InsertAt(int index, T item)
        {
            Fx.Assert(index >= 0 && index <= this.size, "");

            if (this.buffer == null)
            {
                this.buffer = new T[1];
            }
            else if (this.buffer.Length == this.size)
            {
                // PERF, Microsoft, how should we choose a new size?
                T[] tmp = new T[this.size + 1];

                if (index == 0)
                {
                    Array.Copy(this.buffer, 0, tmp, 1, this.size);
                }
                else if (index == this.size)
                {
                    Array.Copy(this.buffer, 0, tmp, 0, this.size);
                }
                else
                {
                    Array.Copy(this.buffer, 0, tmp, 0, index);
                    Array.Copy(this.buffer, index, tmp, index + 1, this.size - index);
                }

                this.buffer = tmp;
            }
            else
            {
                Array.Copy(this.buffer, index, this.buffer, index + 1, this.size - index);
            }

            this.buffer[index] = item;
            ++this.size;
        }

        internal bool Remove(T item)
        {
            int i = IndexOf(item);

            if (i >= 0)
            {
                RemoveAt(i);
                return true;
            }

            return false;
        }

        internal void RemoveAt(int index)
        {
            Fx.Assert(index >= 0 && index < this.size, "");

            if (index < this.size - 1)
            {
                Array.Copy(this.buffer, index + 1, this.buffer, index, this.size - index - 1);
            }

            this.buffer[--this.size] = default(T);
        }

        int Search(T item)
        {
            if (size == 0)
                return ~0;
            return Search(item, Comparer);
        }

        int Search<K>(K key, IItemComparer<K, T> comparer)
        {
            if (this.size <= 8)
            {
                return LinearSearch<K>(key, comparer, 0, this.size);
            }
            else
            {
                return BinarySearch(key, comparer);
            }
        }

        int BinarySearch<K>(K key, IItemComparer<K, T> comparer)
        {
            // [low, high)
            int low = 0;
            int high = this.size;
            int mid, result;

            // Binary search is implemented here so we could look for a type that is different from the
            // buffer type.  Also, the search switches to linear for 8 or fewer elements.
            while (high - low > 8)
            {
                mid = (high + low) / 2;
                result = comparer.Compare(key, this.buffer[mid]);
                if (result < 0)
                {
                    high = mid;
                }
                else if (result > 0)
                {
                    low = mid + 1;
                }
                else
                {
                    return mid;
                }
            }

            return LinearSearch<K>(key, comparer, low, high);
        }

        // [start, bound)
        int LinearSearch<K>(K key, IItemComparer<K, T> comparer, int start, int bound)
        {
            int result;

            for (int i = start; i < bound; ++i)
            {
                result = comparer.Compare(key, this.buffer[i]);
                if (result == 0)
                {
                    return i;
                }

                if (result < 0)
                {
                    // Return the bitwise negation of the insertion index
                    return ~i;
                }
            }

            // Return the bitwise negation of the insertion index
            return ~bound;
        }
#if NO
        internal T[] ToArray()
        {
            T[] tmp = new T[this.size];
            Array.Copy(this.buffer, 0, tmp, 0, this.size);
            return tmp;
        }
#endif
        internal void Trim()
        {
            this.Capacity = this.size;
        }

        internal class DefaultComparer : IItemComparer<T, T>
        {
            public static IComparer<T> Comparer;

            public DefaultComparer(C comparer)
            {
                Comparer = comparer;
            }

            public int Compare(T item1, T item2)
            {
                return Comparer.Compare(item1, item2);
            }
        }
    }

    internal interface IItemComparer<K, V>
    {
        int Compare(K key, V value);
    }
}
