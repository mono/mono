//------------------------------------------------------------------------------
// <copyright file="AspNetWebSocketManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.WebSockets {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Util;

    // Keeps track of AspNetWebSocket instances so that they can be aborted en masse,
    // such as in the case of an AppDomain shutdown.

    internal sealed class AspNetWebSocketManager {

        public static readonly AspNetWebSocketManager Current = new AspNetWebSocketManager(PerfCounters.Instance);

        private bool _aborted;
        internal readonly HashSet<IAsyncAbortableWebSocket> _activeSockets = new HashSet<IAsyncAbortableWebSocket>(); // internal only for unit testing purposes
        private readonly IPerfCounters _perfCounters;

        internal AspNetWebSocketManager(IPerfCounters perfCounters) {
            _perfCounters = perfCounters;
        }

        public int ActiveSocketCount {
            get {
                // We acquire a full lock when reading the count, similar to how the collections
                // in the System.Collections.Concurrent namespace operate.
                lock (_activeSockets) {
                    return _activeSockets.Count;
                }
            }
        }

        // Calls Abort() on each tracked socket, then blocks until all have been aborted
        public void AbortAllAndWait() {
            // Make a copy so we're not iterating over the original collection asynchronously;
            // keep the lock for as short a duration as possible.
            IAsyncAbortableWebSocket[] sockets;
            lock (_activeSockets) {
                _aborted = true;
                sockets = _activeSockets.ToArray();
            }

            Task[] abortTasks = Array.ConvertAll(sockets, socket => socket.AbortAsync());
            Task.WaitAll(abortTasks);
        }

        // Begins tracking a socket, calling Abort() if there was an earlier call to AbortAll()
        public void Add(IAsyncAbortableWebSocket webSocket) {
            int activeSocketCount;
            bool shouldAbort;

            // keep the lock for as short a period as possible
            lock (_activeSockets) {
                _activeSockets.Add(webSocket);
                activeSocketCount = _activeSockets.Count;
                shouldAbort = _aborted;
            }

            // perform any additional operations outside the lock
            _perfCounters.SetCounter(AppPerfCounter.REQUESTS_EXECUTING_WEBSOCKETS, activeSocketCount);
            if (shouldAbort) {
                webSocket.AbortAsync(); // don't care about the result of the abort at the present time
            }
        }

        // Stops tracking a socket
        public void Remove(IAsyncAbortableWebSocket webSocket) {
            int activeSocketCount;

            // keep the lock for as short a period as possible
            lock (_activeSockets) {
                _activeSockets.Remove(webSocket);
                activeSocketCount = _activeSockets.Count;
            }

            // perform any additional operations outside the lock
            _perfCounters.SetCounter(AppPerfCounter.REQUESTS_EXECUTING_WEBSOCKETS, activeSocketCount);
        }

    }
}
