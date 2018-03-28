#pragma warning disable 1634, 1691
using System;
using System.Diagnostics;
using System.Transactions;
using System.Collections;
using System.Collections.Generic;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime
{
    /// <summary>
    /// Volatile Resource Manager
    /// </summary>    
    internal sealed class VolatileResourceManager
    {
        // members
        private WorkBatchCollection _workCollection = new WorkBatchCollection();
        private WorkBatch _mergedBatch = null;

        // constructor
        internal VolatileResourceManager()
        {
        }

        // properties
        internal WorkBatchCollection BatchCollection
        {
            get
            {
                return _workCollection;
            }
        }

        internal bool IsBatchDirty
        {
            get
            {
                IDictionaryEnumerator de = _workCollection.GetEnumerator();
                while (de.MoveNext())
                {
                    WorkBatch batch = (WorkBatch)de.Value;
                    if (batch.IsDirty)
                        return true;
                }
                return false;
            }
        }

        WorkBatch GetMergedBatch()
        {
            return this._workCollection.GetMergedBatch();
        }

        internal void Commit()
        {
            _mergedBatch = GetMergedBatch();

            Transaction transaction = Transaction.Current;
            if (null == transaction)
                throw new InvalidOperationException(ExecutionStringManager.NullAmbientTransaction);

            // Do Commit Sequence iteration over work collection
            _mergedBatch.Commit(transaction);
        }

        internal void Complete()
        {
            try
            {
                _mergedBatch.Complete(true);
            }
            finally
            {
                if (_mergedBatch != null)
                {
                    _mergedBatch.Dispose();
                    _mergedBatch = null;
                }
                if (_workCollection != null)
                {
                    _workCollection.ClearSubBatches();
                }
            }
        }

        internal void HandleFault()
        {
            //
            // We've failed, clear the merged batch
            if (_mergedBatch != null)
            {
                _mergedBatch.Dispose();
                _mergedBatch = null;
            }
            // clear transient batch which holds instance state primarily
            if (_workCollection != null)
            {
                _workCollection.ClearTransientBatch();
            }
        }

        internal void ClearAllBatchedWork()
        {
            if (_workCollection != null)
                _workCollection.RollbackAllBatchedWork();
        }
    }
}
