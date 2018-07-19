//------------------------------------------------------------------------------
// <copyright file="PageAsyncTaskApm.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    // Represents an IPageAsyncTask that follows APM (Begin / End)

    internal sealed class PageAsyncTaskApm : IPageAsyncTask {

        private readonly BeginEventHandler _beginHandler;
        private readonly EndEventHandler _endHandler;
        private readonly object _state;

        public PageAsyncTaskApm(BeginEventHandler beginHandler, EndEventHandler endHandler, object state) {
            _beginHandler = beginHandler;
            _endHandler = endHandler;
            _state = state;
        }

        public async Task ExecuteAsync(object sender, EventArgs e, CancellationToken cancellationToken) {
            // The CancellationToken is ignored in APM since every call to Begin must be matched by
            // a call to End, otherwise memory leaks and other badness can occur.

            // The reason we don't use TaskFactory.FromAsync is that we need the end handler to execute
            // within the current synchronization context, but FromAsync doesn't make that guarantee.

            // The callback that marks the TaskCompletionSource as complete can execute synchronously or asynchronously.
            TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
            IAsyncResult asyncResult = _beginHandler(sender, e, _ => { taskCompletionSource.SetResult(null); }, _state);

            await taskCompletionSource.Task;
            _endHandler(asyncResult);
        }

    }
}
