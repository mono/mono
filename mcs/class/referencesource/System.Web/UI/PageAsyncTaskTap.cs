//------------------------------------------------------------------------------
// <copyright file="PageAsyncTaskTap.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    // Represents an IPageAsyncTask that follows TAP (Task-returning Async method)

    internal sealed class PageAsyncTaskTap : IPageAsyncTask {

        private readonly Func<CancellationToken, Task> _handler;

        public PageAsyncTaskTap(Func<CancellationToken, Task> handler) {
            _handler = handler;
        }

        public Task ExecuteAsync(object sender, EventArgs e, CancellationToken cancellationToken) {
            return _handler(cancellationToken);
        }

    }
}
