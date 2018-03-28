//------------------------------------------------------------------------------
// <copyright file="PageAsyncTaskManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    // Handles execution of asynchronous tasks that are registered with a Page

    internal sealed class PageAsyncTaskManager {

        private bool _executeTasksAsyncHasCompleted = false;
        private readonly Queue<IPageAsyncTask> _registeredTasks = new Queue<IPageAsyncTask>();

        public void EnqueueTask(IPageAsyncTask task) {
            if (_executeTasksAsyncHasCompleted) {
                // don't allow multiple calls to the execution routine
                throw new InvalidOperationException(SR.GetString(SR.PageAsyncManager_CannotEnqueue));
            }

            _registeredTasks.Enqueue(task);
        }

        public async Task ExecuteTasksAsync(object sender, EventArgs e, CancellationToken cancellationToken, AspNetSynchronizationContextBase syncContext, IRequestCompletedNotifier requestCompletedNotifier) {
            try {
                while (_registeredTasks.Count > 0) {
                    // if canceled, propagate exception to caller and stop executing tasks
                    cancellationToken.ThrowIfCancellationRequested();

                    // if request finished, stop executing tasks
                    if (requestCompletedNotifier.IsRequestCompleted) {
                        return;
                    }

                    // execute this task
                    IPageAsyncTask task = _registeredTasks.Dequeue();
                    using (syncContext.AllowVoidAsyncOperationsBlock()) {
                        await task.ExecuteAsync(sender, e, cancellationToken);
                    }
                }
            }
            finally {
                _executeTasksAsyncHasCompleted = true;
            }
        }

    }
}
