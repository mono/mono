//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Threading;
    using System.Windows.Threading;

    internal class TaskDispatcher
    {
        // We need to keep a reference to the WPF Dispatcher so that we can dispatch work to the UI Thread.
        private Dispatcher dispatcher;

        // This constructor must be executed on the UI thread.
        internal TaskDispatcher()
        {
            this.dispatcher = Dispatcher.CurrentDispatcher;
        }

        internal virtual void DispatchWorkOnUIThread(DispatcherPriority priority, Delegate method)
        {
            this.dispatcher.BeginInvoke(priority, method);
        }

        internal virtual void DispatchWorkOnBackgroundThread(WaitCallback work, object state)
        {
            ThreadPool.QueueUserWorkItem(work, state);
        }
    }
}
