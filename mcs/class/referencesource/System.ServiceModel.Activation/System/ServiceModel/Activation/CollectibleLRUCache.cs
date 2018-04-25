//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Diagnostics;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    // This class implements an LRU cache that support recycling of oldest items.
    //
    // The read path is very light-weighted. It takes the reader lock, do a cache lookup, and increment the counter to 
    // make sure the item is up-to-date.
    //
    // It exposes the writer lock through an IDisposable object through CreateWriterLockScope(). Whenever a modification
    // is required to the cache, we create a scope to perform the work.
    // 
    // It exposes a list of "unsafe" methods for cache modifications. These operations should be invoked only inside a 
    // WriterLockScope.
    //
    // Recycling happens in batches. The method UnsafeBeginBatchCollect() finds a batch of items, remove them from the 
    // cache, and have them closed. The counter is updated whenever Collect happens.
    //
    // It supports the recycling for the whole cache. In order to avoid blocking the closing, the asynchronous method
    // UnsafeBeginCollectAll() is used to initiate the close operations for all of the nodes.
    // 
    // Since the cache favors reads than writes, the items in the cache are not sorted until a Collect operation happens.
    // When the Collect operation happens, items are sorted by the LastCounter field of CollectibleNode. Oldest items which
    // are collectible (CanClose returns true) are moved into the batch for collection.
    //
    // Here are some fields of the class that control the recycling logic to achieve best results:
    // - collectPercentageInOneBatch: This defines how many items the batch can have for a single Collect operation.
    //   We need to best leverage the machine capacity but at the same time have an efficient recycling result. This
    //   number defines the percentage of items in the cache to be collected. The value is hard-coded to be 25%.
    // - minSkipCountForWrites: This defines the consecutive writes (service activation, for example) before the next 
    // Collect operation.
    class CollectibleLRUCache<TKey, TValue>
    {
        // Collect x% of items when a collection happens
        readonly double collectPercentageInOneBatch = 0.25;

        // After an immediate collection, we skip this number of new writes before performing another collection
        readonly int minSkipCountForWrites = 4;

        // The look up counter that simulates the timestamp
        int counter;

        ReaderWriterLockSlim rwLock;

        // Records the current counter for write
        int writeCounter;

        Dictionary<TKey, CollectibleNode> directory;
        CollectibleBatch currentCollectibleBatch;

        public CollectibleLRUCache(int capacity, IEqualityComparer<TKey> comparer)
        {
            rwLock = new ReaderWriterLockSlim();
            directory = new Dictionary<TKey, CollectibleNode>(capacity, comparer);
            currentCollectibleBatch = new CollectibleBatch();
        }

        public CollectibleNode this[TKey key]
        {
            get
            {
                rwLock.EnterReadLock();
                try
                {
                    CollectibleNode node = UnsafeGet(key);
                    if (node != null)
                    {
                        // Record the last counter in the node. We don't take a writer lock here because the counter does 
                        // not have to be very accurate.
                        node.LastCounter = Interlocked.Increment(ref counter);
                    }

                    return node;
                }
                finally
                {
                    rwLock.ExitReadLock();
                }
            }
        }

        // This method must be called inside a lock. The counter is not updated
        public CollectibleNode UnsafeGet(TKey key)
        {
            CollectibleNode node;
            if (directory.TryGetValue(key, out node))
            {
                return node;
            }

            return node;
        }

        public void Touch(TKey key)
        {
            rwLock.EnterReadLock();
            try
            {
                CollectibleNode node = UnsafeGet(key);

                if (node != null)
                {
                    // Record the last counter in the node. We don't take a writer lock here because the counter does 
                    // not have to be very accurate. 
                    node.LastCounter = Interlocked.Increment(ref counter);
                }
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        public void UnsafeRemove(CollectibleNode node)
        {
            if (directory.ContainsKey(node.GetKey()))
            {
                directory.Remove(node.GetKey());
            }
        }

        // This method must be called inside a writable lock
        public void UnsafeAdd(CollectibleNode node)
        {
            Fx.Assert(rwLock.IsWriteLockHeld, "This method can be called only when the WriterLock is acquired");

            writeCounter++;
            directory.Add(node.GetKey(), node);
            node.LastCounter = Interlocked.Increment(ref counter);
        }

        // This method must be called inside a writable lock
        public bool UnsafeBeginBatchCollect()
        {
            return UnsafeBeginBatchCollect(false);
        }

        public bool UnsafeBeginBatchCollect(bool collectingAll)
        {
            Fx.Assert(rwLock.IsWriteLockHeld, "This method can be called only when the WriterLock is acquired");

            if (collectingAll)
            {
                AbortExistingBatch();

                if (this.directory.Count > 0)
                {
                    this.currentCollectibleBatch.AddRange(this.directory.Values);
                    this.directory.Clear();
                }
            }
            else
            {
                // We need to avoid collecting items in a consecutive order.
                if (minSkipCountForWrites >= writeCounter)
                {
                    return true;
                }

                CollectibleNode[] array = ResetCountersAndToArray();
                Array.Sort<CollectibleNode>(array, CollectibleNode.CounterComparison);

                // Collect the items here.
                int collectTargetCount = (int)(array.Length * collectPercentageInOneBatch);
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].CanClose())
                    {
                        currentCollectibleBatch.Add(array[i]);
                    }

                    if (currentCollectibleBatch.Count >= collectTargetCount)
                    {
                        break;
                    }
                }

                if (currentCollectibleBatch.Count == 0)
                {
                    return false;
                }

                for (int i = 0; i < currentCollectibleBatch.Count; i++)
                {
                    directory.Remove(currentCollectibleBatch[i].GetKey());
                }
            }

            currentCollectibleBatch.BeginCollect();

            // Wrapping WriterCounter to 0 to avoid integer overflow.
            writeCounter = 0;

            return true;
        }

        [SuppressMessage(FxCop.Category.Reliability, FxCop.Rule.AvoidCallingProblematicMethods,
            Justification = "Calling GC.Collect to control memory usage more explicitly.")]        
        public void EndBatchCollect()
        {
            currentCollectibleBatch.WaitForRecyclingCompletion();

            // Force garbage collection
            GC.Collect(2, GCCollectionMode.Optimized);
        }

        public void Abort()
        {
            using (this.CreateWriterLockScope())
            {
                AbortExistingBatch();

                if (this.directory.Count > 0)
                {
                    this.currentCollectibleBatch.AddRange(this.directory.Values);
                    this.currentCollectibleBatch.Abort();
                }
            }
        }

        void AbortExistingBatch()
        {
            if (this.currentCollectibleBatch.Count > 0)
            {
                this.currentCollectibleBatch.Abort();
            }
        }

        public int Count
        {
            get
            {
                return this.directory.Count;
            }
        }

        public IDisposable CreateWriterLockScope()
        {
            return new WriterLockScope(this.rwLock);
        }

        CollectibleNode[] ResetCountersAndToArray()
        {
            CollectibleNode[] array = directory.Values.ToArray();

            // Reset the counters so that the integer counters are not wrapped (overflow from positive to negative)
            for (int i = 0; i < array.Length; i++)
            {
                array[i].LastCounter -= this.counter;
            }

            this.counter = 0;
            return array;
        }

        internal abstract class CollectibleNode
        {
            public static Comparison<CollectibleNode> CounterComparison = new Comparison<CollectibleNode>(CounterLessThan);
            public int LastCounter;
            public abstract TKey GetKey();
            public abstract IAsyncResult BeginClose(AsyncCallback callback, object state);
            public abstract void EndClose(IAsyncResult result);
            public abstract void Abort();
            public abstract bool CanClose();
            public TValue Value { get; set; }

            public static int CounterLessThan(CollectibleNode x, CollectibleNode y)
            {
                return x.LastCounter - y.LastCounter;
            }
        }

        class WriterLockScope : IDisposable
        {
            ReaderWriterLockSlim rwLock;
            public WriterLockScope(ReaderWriterLockSlim rwLock)
            {
                this.rwLock = rwLock;
                rwLock.EnterWriteLock();
            }

            public void Dispose()
            {
                rwLock.ExitWriteLock();
            }
        }

        class CollectibleBatch : List<CollectibleNode>
        {
            ManualResetEvent recyclingCompletedWaitHandle;
            AsyncCallback collectibleNodeClosedCallback;
            int totalCollectCount;

            public CollectibleBatch()
            {
                // The event is initially set when the batch is empty.
                recyclingCompletedWaitHandle = new ManualResetEvent(true);
                collectibleNodeClosedCallback = Fx.ThunkCallback(new AsyncCallback(OnCollectibleNodeClosed));
            }

            public void BeginCollect()
            {
                this.totalCollectCount = this.Count;
                if (this.totalCollectCount == 0)
                {
                    return;
                }

                recyclingCompletedWaitHandle.Reset();
                for (int i = 0; i < this.Count; i++)
                {
                    IAsyncResult result = this[i].BeginClose(collectibleNodeClosedCallback, this[i]);
                    if (result == null)
                    {
                        DecrementCollectCount();
                    }
                    else if (result.CompletedSynchronously)
                    {
                        HandleCollectibleNodeClosed(result);
                    }
                }
            }

            public void WaitForRecyclingCompletion()
            {
                recyclingCompletedWaitHandle.WaitOne();
                this.Clear();
            }

            public void Abort()
            {
                for (int i = 0; i < this.Count; i++)
                {
                    this[i].Abort();
                }

                this.Clear();
                recyclingCompletedWaitHandle.Set();
            }

            void DecrementCollectCount()
            {
                int currentCount = Interlocked.Decrement(ref this.totalCollectCount);
                if (currentCount == 0)
                {
                    this.recyclingCompletedWaitHandle.Set();
                }
            }

            void OnCollectibleNodeClosed(IAsyncResult result)
            {
                if (result == null || result.CompletedSynchronously)
                {
                    return;
                }

                HandleCollectibleNodeClosed(result);
            }

            void HandleCollectibleNodeClosed(IAsyncResult result)
            {
                CollectibleNode node = result.AsyncState as CollectibleNode;
                if (node != null)
                {
                    node.EndClose(result);
                }

                DecrementCollectCount();
            }

        }
    }
}
