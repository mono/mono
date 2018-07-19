//------------------------------------------------------------------------------
// <copyright file="TaskWrapperAsyncResult.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Wraps a Task class, optionally overriding the State object (since the Task Asynchronous Pattern doesn't normally use them).
 * 
 * Copyright (c) 2010 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class TaskWrapperAsyncResult : IAsyncResult {

        private bool _forceCompletedSynchronously;

        internal TaskWrapperAsyncResult(Task task, object asyncState) {
            Task = task;
            AsyncState = asyncState;
        }

        public object AsyncState {
            get;
            private set;
        }

        public WaitHandle AsyncWaitHandle {
            get { return ((IAsyncResult)Task).AsyncWaitHandle; }
        }

        public bool CompletedSynchronously {
            get { return _forceCompletedSynchronously || ((IAsyncResult)Task).CompletedSynchronously; }
        }

        public bool IsCompleted {
            get { return ((IAsyncResult)Task).IsCompleted; }
        }

        internal Task Task {
            get;
            private set;
        }

        internal void ForceCompletedSynchronously() {
            _forceCompletedSynchronously = true;
        }

    }
}
