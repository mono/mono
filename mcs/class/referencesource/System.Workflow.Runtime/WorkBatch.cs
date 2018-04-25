#pragma warning disable 1634, 1691
using System;
using System.Diagnostics;
using System.Transactions;
using System.Collections;
using System.Collections.Generic;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime
{
    #region Runtime Batch Implementation

    #region WorkBatch

    internal enum WorkBatchState
    {
        Usable,
        Merged,
        Completed
    }

    /// <summary>
    /// Summary description for Work Batching. 
    /// </summary>
    internal sealed class WorkBatch : IWorkBatch, IDisposable
    {
        private PendingWorkCollection _pendingWorkCollection;
        private object mutex = new object();
        private WorkBatchState _state;
        private WorkBatchCollection _collection = null;

        private WorkBatch()
        {
        }

        internal WorkBatch(WorkBatchCollection workBackCollection)
        {
            _pendingWorkCollection = new PendingWorkCollection();
            _state = WorkBatchState.Usable;
            _collection = workBackCollection;
        }

        internal int Count
        {
            get { return _pendingWorkCollection.WorkItems.Count; }
        }

        internal void SetWorkBatchCollection(WorkBatchCollection workBatchCollection)
        {
            _collection = workBatchCollection;
        }

        #region IWorkBatch Implementation
        /// <summary>
        /// Add Work to Batch
        /// </summary>
        /// <param name="work"></param>
        /// <param name="workItem"></param>
        void IWorkBatch.Add(IPendingWork work, object workItem)
        {
            if (_pendingWorkCollection == null)
                throw new ObjectDisposedException("WorkBatch");

            lock (this.mutex)
            {
                System.Diagnostics.Debug.Assert(this._state == WorkBatchState.Usable, "Trying to add to unusable batch.");

                _pendingWorkCollection.Add(work, _collection.GetNextWorkItemOrderId(work), workItem);
            }
        }
        #endregion

        #region Internal Implementation

        internal bool IsDirty
        {
            get
            {
                return this._pendingWorkCollection.IsDirty;
            }
        }
        /// <summary>
        /// This one commits all the pending work and its items 
        /// added so far in this batch.
        /// </summary>
        /// <param name="transaction"></param>
        internal void Commit(Transaction transaction)
        {
            lock (this.mutex)
            {
                _pendingWorkCollection.Commit(transaction);
            }
        }


        /// <summary>
        /// This one completes the pending work
        /// </summary>
        /// <param name="succeeded"></param>
        internal void Complete(bool succeeded)
        {
            lock (this.mutex)
            {
                if (this._pendingWorkCollection.IsUsable)
                {
                    _pendingWorkCollection.Complete(succeeded);
                    _pendingWorkCollection.Dispose();
                    this._state = WorkBatchState.Completed;
                }
            }
        }

        /// <summary>
        /// API for Runtime to call to do Merge Operation: Right now 
        /// we dont use this because we dont support incoming work collection.
        /// </summary>
        /// <param name="batch"></param>
        internal void Merge(WorkBatch batch)
        {
            if (batch == null)
                return; //nothing to merge

            if (_pendingWorkCollection == null)
                throw new ObjectDisposedException("WorkBatch");

            lock (this.mutex)
            {
                lock (batch.mutex)
                {
                    foreach (KeyValuePair<IPendingWork, SortedList<long, object>> item in batch._pendingWorkCollection.WorkItems)
                    {
                        //_pendingWorkCollection.AddRange(item.Key, item.Value);
                        SortedList<long, object> newItems = item.Value;
                        foreach (KeyValuePair<long, object> kvp in newItems)
                            _pendingWorkCollection.Add(item.Key, kvp.Key, kvp.Value);
                    }
                }

                this._state = WorkBatchState.Merged;
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pendingWorkCollection.Dispose();
                _pendingWorkCollection = null;
            }
        }
        #endregion

        #region PendingWorkCollection implementation

        /// <summary>
        /// Pending Work Implementation
        /// </summary>
        internal sealed class PendingWorkCollection : IDisposable
        {
            Dictionary<IPendingWork, SortedList<long, object>> Items;

            #region Internal Implementation
            internal PendingWorkCollection()
            {
                Items = new Dictionary<IPendingWork, SortedList<long, object>>();
            }

            internal Dictionary<IPendingWork, SortedList<long, object>> WorkItems
            {
                get
                {
                    return Items;
                }
            }

            internal bool IsUsable
            {
                get
                {
                    return this.Items != null;
                }
            }

            internal bool IsDirty
            {
                get
                {
                    if (!this.IsUsable)
                        return false;

                    //
                    // Loop through all pending work items in the collection
                    // If any of them assert that they need to commit than the batch is dirty
                    foreach (KeyValuePair<IPendingWork, SortedList<long, object>> workItem in this.WorkItems)
                    {
                        try
                        {
                            IPendingWork work = workItem.Key;
                            if (work.MustCommit(workItem.Value))
                                return true;
                        }
                        catch (Exception e)
                        {
                            if (WorkflowExecutor.IsIrrecoverableException(e))
                            {
#pragma warning disable 56503
                                throw;
#pragma warning restore 56503
                            }
                            else
                            {
                                // Ignore exceptions and treat condition as false return value;
                            }
                        }
                    }
                    //
                    // If no one asserted that they need to commit we're not dirty
                    return false;
                }
            }

            internal void Add(IPendingWork work, long orderId, object workItem)
            {
                SortedList<long, object> workItems = null;

                if (!Items.TryGetValue(work, out workItems))
                {
                    workItems = new SortedList<long, object>();
                    Items.Add(work, workItems);
                }
                Debug.Assert(!workItems.ContainsKey(orderId), string.Format(System.Globalization.CultureInfo.InvariantCulture, "List already contains key {0}", orderId));
                workItems.Add(orderId, workItem);
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "pending work hc {0} added workItem hc {1}", work.GetHashCode(), workItem.GetHashCode());
            }

            //Commit All Pending Work
            internal void Commit(Transaction transaction)
            {
                //ignore items param
                foreach (KeyValuePair<IPendingWork, SortedList<long, object>> workItem in Items)
                {
                    IPendingWork work = workItem.Key;
                    List<object> values = new List<object>(workItem.Value.Values);
                    work.Commit(transaction, values);
                }
            }

            //Complete All Pending Work
            internal void Complete(bool succeeded)
            {
                foreach (KeyValuePair<IPendingWork, SortedList<long, object>> workItem in Items)
                {
                    IPendingWork work = workItem.Key;
                    List<object> values = new List<object>(workItem.Value.Values);
                    try
                    {
                        work.Complete(succeeded, values);
                    }
                    catch (Exception e)
                    {
                        if (WorkflowExecutor.IsIrrecoverableException(e))
                        {
                            throw;
                        }
                        else
                        {
                            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Warning, 0, "Work Item {0} threw exception on complete notification", workItem.GetType());
                        }
                    }
                }
            }

            #endregion

            #region IDisposable Members
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing && Items != null)
                {
                    Items.Clear();
                    Items = null;
                }
            }

            #endregion
        }
        #endregion
    }
    #endregion

    #region WorkBatchCollection
    /// <summary>
    /// collection of name to Batch
    /// </summary>
    internal sealed class WorkBatchCollection : Dictionary<object, WorkBatch>
    {
        object transientBatchID = new object();
        private object mutex = new object();
        //
        // All access must be through Interlocked.* methods
        private long _workItemOrderId = 0;

        internal long WorkItemOrderId
        {
            get
            {
                return Threading.Interlocked.Read(ref _workItemOrderId);
            }
            set
            {
                Debug.Assert(value >= _workItemOrderId, "New value for WorkItemOrderId must be greater than the current value");
                lock (mutex)
                {
                    _workItemOrderId = value;
                }
            }
        }

        internal long GetNextWorkItemOrderId(IPendingWork pendingWork)
        {
            return Threading.Interlocked.Increment(ref _workItemOrderId);
        }
        /// <summary>
        /// A new batch is created per atomic scope or any
        /// required sub batches. An example of an optional sub batch
        /// could be a batch created for Send activities
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal IWorkBatch GetBatch(object id)
        {
            WorkBatch batch = null;

            lock (mutex)
            {
                if (this.TryGetValue(id, out batch))
                    return batch;

                batch = new WorkBatch(this);
                Add(id, batch);
            }

            return batch;
        }

        /// <summary>
        /// Find a batch for a given id without creating it.
        /// </summary>
        /// <param name="id">batch key</param>
        /// <returns>batch or null if not found</returns>
        private WorkBatch FindBatch(object id)
        {
            WorkBatch batch = null;
            lock (mutex)
            {
                TryGetValue(id, out batch);
            }

            return batch;
        }

        internal IWorkBatch GetTransientBatch()
        {
            return GetBatch(transientBatchID);
        }

        internal WorkBatch GetMergedBatch()
        {
            lock (mutex)
            {
                WorkBatch batch = new WorkBatch(this);

                foreach (WorkBatch existingBatch in this.Values)
                {
                    batch.Merge(existingBatch);
                }
                //Copy of all the items merged in one batch.
                //Order is preserved in the same way batches are created.
                return batch;
            }
        }

        internal void RollbackBatch(object id)
        {
            lock (mutex)
            {
                WorkBatch batch = FindBatch(id);
                if (batch != null)
                {
                    batch.Complete(false);
                    batch.Dispose();
                    Remove(id);
                }
            }
        }

        // Rollback all sub batches, calling "complete(false)" on all entries.
        internal void RollbackAllBatchedWork()
        {
            lock (mutex)
            {
                foreach (WorkBatch batch in this.Values)
                {
                    batch.Complete(false);
                    batch.Dispose();
                }
                Clear(); // clear the collection
            }
        }

        // Clear sub batches after successful commit/complete.
        internal void ClearSubBatches()
        {
            lock (mutex)
            {
                foreach (WorkBatch existingBatch in this.Values)
                {
                    existingBatch.Dispose();
                }
                Clear(); // clear the collection
            }
        }

        internal void ClearTransientBatch()
        {
            RollbackBatch(transientBatchID);
        }
    }
    #endregion

    #endregion
}
