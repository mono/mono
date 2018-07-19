// <copyright>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Runtime
{
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class TaskExtensions
    {
        public static IAsyncResult AsAsyncResult<T>(this Task<T> task, AsyncCallback callback, object state)
        {
            if (task == null)
            {
                throw Fx.Exception.ArgumentNull("task");
            }

            if (task.Status == TaskStatus.Created)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.SFxTaskNotStarted));
            }

            var tcs = new TaskCompletionSource<T>(state);

            task.ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        // the use of Task.ContinueWith(,TaskContinuationOptions.OnlyOnRanToCompletion)
                        // can give us a cancelled Task here with no t.Exception.
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(t.Result);
                    }

                    if (callback != null)
                    {
                        callback(tcs.Task);
                    }
                }, 
                TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        public static IAsyncResult AsAsyncResult(this Task task, AsyncCallback callback, object state)
        {
            if (task == null)
            {
                throw Fx.Exception.ArgumentNull("task");
            }

            if (task.Status == TaskStatus.Created)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.SFxTaskNotStarted));
            }

            var tcs = new TaskCompletionSource<object>(state);

            task.ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        // the use of Task.ContinueWith(,TaskContinuationOptions.OnlyOnRanToCompletion)
                        // can give us a cancelled Task here with no t.Exception.
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(null);
                    }

                    if (callback != null)
                    {
                        callback(tcs.Task);
                    }
                }, 
                TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        public static ConfiguredTaskAwaitable SuppressContextFlow(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable<T> SuppressContextFlow<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable ContinueOnCapturedContextFlow(this Task task)
        {
            return task.ConfigureAwait(true);
        }

        public static ConfiguredTaskAwaitable<T> ContinueOnCapturedContextFlow<T>(this Task<T> task)
        {
            return task.ConfigureAwait(true);
        }

        public static void Wait<TException>(this Task task)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                throw Fx.Exception.AsError<TException>(ex);
            }
        }

        public static bool Wait<TException>(this Task task, int millisecondsTimeout)
        {
            try
            {
                return task.Wait(millisecondsTimeout);
            }
            catch (AggregateException ex)
            {
                throw Fx.Exception.AsError<TException>(ex);
            }
        }

        public static bool Wait<TException>(this Task task, TimeSpan timeout)
        {
            try
            {
                if (timeout == TimeSpan.MaxValue)
                {
                    return task.Wait(Timeout.Infinite);
                }
                else
                {
                    return task.Wait(timeout);
                }
            }
            catch (AggregateException ex)
            {
                throw Fx.Exception.AsError<TException>(ex);
            }
        }

        public static void Wait(this Task task, TimeSpan timeout, Action<Exception, TimeSpan, string> exceptionConverter, string operationType)
        {
            bool timedOut = false;

            try
            {
                if (timeout > TimeoutHelper.MaxWait)
                {
                    task.Wait();
                }
                else
                {
                    timedOut = !task.Wait(timeout);
                }
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex) || exceptionConverter == null)
                {
                    throw;
                }

                exceptionConverter(ex, timeout, operationType);
            }

            if (timedOut)
            {
                throw Fx.Exception.AsError(new TimeoutException(InternalSR.TaskTimedOutError(timeout)));
            }
        }

        public static Task<TBase> Upcast<TDerived, TBase>(this Task<TDerived> task) where TDerived : TBase
        {
            return (task.Status == TaskStatus.RanToCompletion) ?
                Task.FromResult((TBase)task.Result) :
                UpcastPrivate<TDerived, TBase>(task);
        }

        private static async Task<TBase> UpcastPrivate<TDerived, TBase>(this Task<TDerived> task) where TDerived : TBase
        {
            return await task.ConfigureAwait(false);
        }
    }
}
