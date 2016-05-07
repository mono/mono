//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System.Xml;
    using System.Collections.Generic;
    using System.Threading;
    using System.Runtime;

    class AsyncInvokeOperation
    {
        object thisLock;

        public AsyncInvokeOperation(SynchronizationContext syncContext)
        {
            Fx.Assert(syncContext != null, "syncContext cannot be null");
            this.SyncContext = syncContext;
            thisLock = new object();
        }

        SynchronizationContext SyncContext
        {
            get;
            set;
        }

        bool Completed
        {
            get;
            set;
        }

        public void OperationStarted()
        {
            this.SyncContext.OperationStarted();
        }

        public void OperationCompleted()
        {
            lock (thisLock)
            {
                Fx.AssertAndThrowFatal(!this.Completed, "Async operation has already been completed");
                this.Completed = true;
            }
            this.SyncContext.OperationCompleted();
        }

        public void PostOperationCompleted(SendOrPostCallback callback, object arg)
        {
            lock (thisLock)
            {
                Fx.AssertAndThrowFatal(!this.Completed, "Async operation has already been completed");
                this.Completed = true;
            }
            Fx.Assert(callback != null, "callback cannot be null");
            this.SyncContext.Post(callback, arg);
            this.SyncContext.OperationCompleted();
        }
    }
}
