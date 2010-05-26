/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc.Async {
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class AsyncManager {

        private readonly SynchronizationContext _syncContext;

        // default timeout is 45 sec
        // from: http://msdn.microsoft.com/en-us/library/system.web.ui.page.asynctimeout.aspx
        private int _timeout = 45 * 1000;

        public AsyncManager()
            : this(null /* syncContext */) {
        }

        public AsyncManager(SynchronizationContext syncContext) {
            _syncContext = syncContext ?? SynchronizationContextUtil.GetSynchronizationContext();

            OutstandingOperations = new OperationCounter();
            OutstandingOperations.Completed += delegate { Finish(); };

            Parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public OperationCounter OutstandingOperations {
            get;
            private set;
        }

        public IDictionary<string, object> Parameters {
            get;
            private set;
        }

        public event EventHandler Finished;

        // the developer may call this function to signal that all operations are complete instead of
        // waiting for the operation counter to reach zero
        public virtual void Finish() {
            EventHandler handler = Finished;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        // executes a callback in the current synchronization context, which gives access to HttpContext and related items
        public virtual void Sync(Action action) {
            _syncContext.Sync(action);
        }

        // measured in milliseconds, Timeout.Infinite means 'no timeout'
        public int Timeout {
            get {
                return _timeout;
            }
            set {
                if (value < -1) {
                    throw Error.AsyncCommon_InvalidTimeout("value");
                }
                _timeout = value;
            }
        }

    }
}
