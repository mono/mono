// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// TaskExtensions.cs
//
// <OWNER>Microsoft</OWNER>
//
// Extensions to Task/Task<TResult> classes
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
#if !SILVERLIGHT || FEATURE_NETCORE // Desktop and CoreSys but not CoreCLR
using System.Runtime.ExceptionServices;
#endif

namespace System.Threading.Tasks
{
    /// <summary>
    /// Provides a set of static (Shared in Visual Basic) methods for working with specific kinds of 
    /// <see cref="System.Threading.Tasks.Task"/> instances.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Creates a proxy <see cref="System.Threading.Tasks.Task">Task</see> that represents the 
        /// asynchronous operation of a Task{Task}.
        /// </summary>
        /// <remarks>
        /// It is often useful to be able to return a Task from a <see cref="System.Threading.Tasks.Task{TResult}">
        /// Task{TResult}</see>, where the inner Task represents work done as part of the outer Task{TResult}.  However, 
        /// doing so results in a Task{Task}, which, if not dealt with carefully, could produce unexpected behavior.  Unwrap 
        /// solves this problem by creating a proxy Task that represents the entire asynchronous operation of such a Task{Task}.
        /// </remarks>
        /// <param name="task">The Task{Task} to unwrap.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown if the 
        /// <paramref name="task"/> argument is null.</exception>
        /// <returns>A Task that represents the asynchronous operation of the provided Task{Task}.</returns>
        public static Task Unwrap(this Task<Task> task)
        {
            if (task == null) throw new ArgumentNullException("task");
#if SILVERLIGHT && !FEATURE_NETCORE // CoreCLR only
            bool result;

            // tcs.Task serves as a proxy for task.Result.
            // AttachedToParent is the only legal option for TCS-style task.
            var tcs = new TaskCompletionSource<Task>(task.CreationOptions & TaskCreationOptions.AttachedToParent);

            // Set up some actions to take when task has completed.
            task.ContinueWith(delegate
            {
                switch (task.Status)
                {
                    // If task did not run to completion, then record the cancellation/fault information
                    // to tcs.Task.
                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                        result = tcs.TrySetFromTask(task);
                        Contract.Assert(result, "Unwrap(Task<Task>): Expected TrySetFromTask #1 to succeed");
                        break;

                    case TaskStatus.RanToCompletion:
                        // task.Result == null ==> proxy should be canceled.
                        if (task.Result == null) tcs.TrySetCanceled();

                        // When task.Result completes, take some action to set the completion state of tcs.Task.
                        else
                        {
                            task.Result.ContinueWith(_ =>
                            {
                                // Copy completion/cancellation/exception info from task.Result to tcs.Task.
                                result = tcs.TrySetFromTask(task.Result);
                                Contract.Assert(result, "Unwrap(Task<Task>): Expected TrySetFromTask #2 to succeed");
                            }, TaskContinuationOptions.ExecuteSynchronously).ContinueWith(antecedent =>
                            {
                                // Clean up if ContinueWith() operation fails due to TSE
                                tcs.TrySetException(antecedent.Exception);
                            }, TaskContinuationOptions.OnlyOnFaulted);
                        }
                        break;
                }
            }, TaskContinuationOptions.ExecuteSynchronously).ContinueWith(antecedent =>
            {
                // Clean up if ContinueWith() operation fails due to TSE
                tcs.TrySetException(antecedent.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);

            // Return this immediately as a proxy.  When task.Result completes, or task is faulted/canceled,
            // the completion information will be transfered to tcs.Task.
            return tcs.Task;

#else // Desktop or CoreSys
            // Creates a proxy Task and hooks up the logic to have it represent the task.Result
            Task promise = Task.CreateUnwrapPromise<VoidResult>(task, lookForOce : false);
            
            // Return the proxy immediately
            return promise;
#endif
        }

        /// <summary>
        /// Creates a proxy <see cref="System.Threading.Tasks.Task{TResult}">Task{TResult}</see> that represents the 
        /// asynchronous operation of a Task{Task{TResult}}.
        /// </summary>
        /// <remarks>
        /// It is often useful to be able to return a Task{TResult} from a Task{TResult}, where the inner Task{TResult} 
        /// represents work done as part of the outer Task{TResult}.  However, doing so results in a Task{Task{TResult}}, 
        /// which, if not dealt with carefully, could produce unexpected behavior.  Unwrap solves this problem by 
        /// creating a proxy Task{TResult} that represents the entire asynchronous operation of such a Task{Task{TResult}}.
        /// </remarks>
        /// <param name="task">The Task{Task{TResult}} to unwrap.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown if the 
        /// <paramref name="task"/> argument is null.</exception>
        /// <returns>A Task{TResult} that represents the asynchronous operation of the provided Task{Task{TResult}}.</returns>        
        public static Task<TResult> Unwrap<TResult>(this Task<Task<TResult>> task)
        {
            if (task == null) throw new ArgumentNullException("task");
#if SILVERLIGHT && !FEATURE_NETCORE // CoreCLR only
            bool result;

            // tcs.Task serves as a proxy for task.Result.
            // AttachedToParent is the only legal option for TCS-style task.
            var tcs = new TaskCompletionSource<TResult>(task.CreationOptions & TaskCreationOptions.AttachedToParent);

            // Set up some actions to take when task has completed.
            task.ContinueWith(delegate
            {
                switch (task.Status)
                {
                    // If task did not run to completion, then record the cancellation/fault information
                    // to tcs.Task.
                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                        result = tcs.TrySetFromTask(task);
                        Contract.Assert(result, "Unwrap(Task<Task<T>>): Expected TrySetFromTask #1 to succeed");
                        break;

                    case TaskStatus.RanToCompletion:
                        // task.Result == null ==> proxy should be canceled.
                        if (task.Result == null) tcs.TrySetCanceled();

                        // When task.Result completes, take some action to set the completion state of tcs.Task.
                        else
                        {
                            task.Result.ContinueWith(_ =>
                            {
                                // Copy completion/cancellation/exception info from task.Result to tcs.Task.
                                result = tcs.TrySetFromTask(task.Result);
                                Contract.Assert(result, "Unwrap(Task<Task<T>>): Expected TrySetFromTask #2 to succeed");
                            },
                            TaskContinuationOptions.ExecuteSynchronously).ContinueWith(antecedent =>
                            {
                                // Clean up if ContinueWith() operation fails due to TSE
                                tcs.TrySetException(antecedent.Exception);
                            }, TaskContinuationOptions.OnlyOnFaulted);
                        }

                        break;
                }
            }, TaskContinuationOptions.ExecuteSynchronously).ContinueWith(antecedent =>
            {
                // Clean up if ContinueWith() operation fails due to TSE
                tcs.TrySetException(antecedent.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted); ;

            // Return this immediately as a proxy.  When task.Result completes, or task is faulted/canceled,
            // the completion information will be transfered to tcs.Task.
            return tcs.Task;

#else // Desktop or CoreSys
            // Creates a proxy Task<TResult> and hooks up the logic to have it represent the task.Result
            Task<TResult> promise = Task.CreateUnwrapPromise<TResult>(task, lookForOce : false);

            // Return the proxy immediately
            return promise;
#endif
        }

#if SILVERLIGHT && !FEATURE_NETCORE // CoreCLR only
        // Transfer the completion status from "source" to "me".
        private static bool TrySetFromTask<TResult>(this TaskCompletionSource<TResult> me, Task source)
        {
            Contract.Assert(source.IsCompleted, "TrySetFromTask: Expected source to have completed.");
            bool rval = false;

            switch(source.Status)
            {
                case TaskStatus.Canceled:
                    rval = me.TrySetCanceled();
                    break;

                case TaskStatus.Faulted:
                    rval = me.TrySetException(source.Exception.InnerExceptions);
                    break;

                case TaskStatus.RanToCompletion:
                    if(source is Task<TResult>)
                        rval = me.TrySetResult( ((Task<TResult>)source).Result);
                    else
                        rval = me.TrySetResult(default(TResult));
                    break;
            }

            return rval;
        }
#else // Desktop or CoreSys

        // Used as a placeholder TResult to indicate that a Task<TResult> has a void TResult
        private struct VoidResult { }

#endif

    }
}
