//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;

    // This is the base object pool class which manages objects in a FIFO queue. The objects are 
    // created through the provided Func<T> createObjectFunc. The main purpose for this class is
    // to get better memory usage for Garbage Collection (GC) when part or all of an object is
    // regularly pinned. Constantly creating such objects can cause large Gen0 Heap fragmentation
    // and thus high memory usage pressure. The pooled objects are first created in Gen0 heaps and
    // would be eventually moved to a more stable segment which would prevent the fragmentation
    // to happen.
    //
    // The objects are created in batches for better localization of the objects. Here are the
    // parameters that control the behavior of creation/removal:
    // 
    // batchAllocCount: number of objects to be created at the same time when new objects are needed
    //
    // createObjectFunc: func delegate that is used to create objects by sub-classes.
    //
    // maxFreeCount: max number of free objects the queue can store. This is to make sure the memory
    //     usage is bounded.
    //
    abstract class QueuedObjectPool<T>
    {
        Queue<T> objectQueue;
        bool isClosed;
        int batchAllocCount;
        int maxFreeCount;

        protected void Initialize(int batchAllocCount, int maxFreeCount)
        {
            if (batchAllocCount <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("batchAllocCount"));
            }

            Fx.Assert(batchAllocCount <= maxFreeCount, "batchAllocCount cannot be greater than maxFreeCount");
            this.batchAllocCount = batchAllocCount;
            this.maxFreeCount = maxFreeCount;
            this.objectQueue = new Queue<T>(batchAllocCount);
        }

        object ThisLock
        {
            get
            {
                return this.objectQueue;
            }
        }

        public virtual bool Return(T value)
        {
            lock (ThisLock)
            {
                if (this.objectQueue.Count < this.maxFreeCount && ! this.isClosed)
                {
                    this.objectQueue.Enqueue(value);
                    return true;
                }

                return false;
            }
        }

        public T Take()
        {
            lock (ThisLock)
            {
                Fx.Assert(!this.isClosed, "Cannot take an item from closed QueuedObjectPool");

                if (this.objectQueue.Count == 0)
                {
                    AllocObjects();
                }

                return this.objectQueue.Dequeue();
            }
        }

        public void Close()
        {
            lock (ThisLock)
            {
                foreach (T item in this.objectQueue)
                {
                    if (item != null)
                    {
                        this.CleanupItem(item);
                    }
                }

                this.objectQueue.Clear();
                this.isClosed = true;
            }
        }

        protected virtual void CleanupItem(T item)
        {
        }

        protected abstract T Create();

        void AllocObjects()
        {
            Fx.Assert(this.objectQueue.Count == 0, "The object queue must be empty for new allocations");
            for (int i = 0; i < batchAllocCount; i++)
            {
                this.objectQueue.Enqueue(Create());
            }
        }
    }
}
