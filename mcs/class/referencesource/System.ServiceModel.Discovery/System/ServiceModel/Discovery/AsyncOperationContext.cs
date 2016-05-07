//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Threading;
    using System.Xml;

    // WARNING: This object is not thread safe. 
    // Use SyncRoot to protect access to methods and properties as required.
    abstract class AsyncOperationContext
    {
        AsyncOperation asyncOperation;
        TimeSpan duration;        
        bool isCompleted;
        int maxResults;
        UniqueId operationId;
        Nullable<DateTime> startTime;

        [Fx.Tag.SynchronizationObject()]
        object syncRoot;

        IOThreadTimer timer;
        object userState;

        internal AsyncOperationContext(UniqueId operationId, int maxResults, TimeSpan duration, object userState)
        {
            Fx.Assert(operationId != null, "The operation id must be non null.");
            Fx.Assert(maxResults > 0, "The maxResults parameter must be positive.");
            Fx.Assert(duration > TimeSpan.Zero, "The duration parameter must be positive.");

            this.maxResults = maxResults;
            this.duration = duration;            
            this.userState = userState;
            this.operationId = operationId;
            this.syncRoot = new object();
        }

        public AsyncOperation AsyncOperation
        {
            get
            {
                return this.asyncOperation;
            }
            set
            {
                this.asyncOperation = value;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return this.duration;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return this.isCompleted;
            }
        }

        public bool IsSyncOperation
        {
            get
            {
                return (UserState is SyncOperationState);
            }
        }

        public int MaxResults
        {
            get
            {
                return this.maxResults;
            }
        }

        public UniqueId OperationId
        {
            get
            {
                return this.operationId;
            }
        }

        public object SyncRoot
        {
            get
            {
                return syncRoot;
            }
        }

        public object UserState
        {
            get
            {
                return this.userState;
            }
        }

        public Nullable<DateTime> StartedAt
        {
            get
            {
                return this.startTime;
            }
        }

        public void Complete()
        {
            this.StopTimer();
            this.isCompleted = true;
        }

        public void StartTimer(Action<object> waitCallback)
        {
            Fx.Assert(this.timer == null, "The timer object must be null.");
            Fx.Assert(this.isCompleted == false, "The timer cannot be started if the context is closed.");

            this.startTime = DateTime.UtcNow;
            this.timer = new IOThreadTimer(waitCallback, this, false);
            this.timer.Set(this.Duration);
        }

        void StopTimer()
        {
            if (this.timer != null)
            {
                this.timer.Cancel();
                this.timer = null;
            }
        }
    }
}
