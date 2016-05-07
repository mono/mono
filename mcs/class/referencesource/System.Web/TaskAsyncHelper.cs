//------------------------------------------------------------------------------
// <copyright file="TaskAsyncHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Assists in converting an method written using the Task Asynchronous Pattern to a Begin/End method pair.
 * 
 * Copyright (c) 2010 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.Threading.Tasks;

    internal static class TaskAsyncHelper {

        internal static IAsyncResult BeginTask(Func<Task> taskFunc, AsyncCallback callback, object state) {
            Task task = taskFunc();
            if (task == null) {
                // Something went wrong - let our caller handle it.
                return null;
            }

            // We need to wrap the inner Task so that the IAsyncResult exposed by this method
            // has the state object that was provided as a parameter. We could be a bit smarter
            // about this to save an allocation if the state objects are equal, but that's a
            // micro-optimization.
            TaskWrapperAsyncResult resultToReturn = new TaskWrapperAsyncResult(task, state);

            // Task instances are always marked CompletedSynchronously = false, even if the
            // operation completed synchronously. We should detect this and modify the IAsyncResult
            // we pass back to our caller as appropriate. Only read the 'IsCompleted' property once
            // to avoid a race condition where the underlying Task completes during this method.
            bool actuallyCompletedSynchronously = task.IsCompleted;
            if (actuallyCompletedSynchronously) {
                resultToReturn.ForceCompletedSynchronously();
            }

            if (callback != null) {
                // ContinueWith() is a bit slow: it captures execution context and hops threads. We should
                // avoid calling it and just invoke the callback directly if the underlying Task is
                // already completed. Only use ContinueWith as a fallback. There's technically a ---- here
                // in that the Task may have completed between the check above and the call to
                // ContinueWith below, but ContinueWith will do the right thing in both cases.
                if (actuallyCompletedSynchronously) {
                    callback(resultToReturn);
                }
                else {
                    task.ContinueWith(_ => callback(resultToReturn));
                }
            }

            return resultToReturn;
        }

        // The parameter is named 'ar' since it matches the parameter name on the EndEventHandler delegate type,
        // and we expect that most consumers will end up invoking this method via an instance of that delegate.
        internal static void EndTask(IAsyncResult ar) {
            if (ar == null) {
                throw new ArgumentNullException("ar");
            }

            // Make sure the incoming parameter is actually the correct type.
            TaskWrapperAsyncResult taskWrapper = ar as TaskWrapperAsyncResult;
            if (taskWrapper == null) {
                // extraction failed
                throw new ArgumentException(SR.GetString(SR.TaskAsyncHelper_ParameterInvalid), "ar");
            }

            // The End* method doesn't actually perform any actual work, but we do need to maintain two invariants:
            // 1. Make sure the underlying Task actually *is* complete.
            // 2. If the Task encountered an exception, observe it here.
            // (TaskAwaiter.GetResult() handles both of those, and it rethrows the original exception rather than an AggregateException.)
            taskWrapper.Task.GetAwaiter().GetResult();
        }

    }
}
