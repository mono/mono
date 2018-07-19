//------------------------------------------------------------------------------
// <copyright file="TaskExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Threading.Tasks;

    // Contains helper methods for dealing with Tasks

    internal static class TaskExtensions {

        // Throws the exception that faulted a Task, similar to what 'await' would have done.
        // Useful for synchronous methods which have a Task instance they know to be already completed
        // and where they want to let the exception propagate upward.
        public static void ThrowIfFaulted(this Task task) {
            Debug.Assert(task.IsCompleted, "The Task passed to this method must be marked as completed so that this method doesn't block.");
            task.GetAwaiter().GetResult();
        }

        // Gets a WithinCancellableCallbackTaskAwaiter from a Task.
        public static WithinCancellableCallbackTaskAwaitable WithinCancellableCallback(this Task task, HttpContext context) {
            return new WithinCancellableCallbackTaskAwaitable(context, task.GetAwaiter());
        }

    }
}
