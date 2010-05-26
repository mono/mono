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
    using System.Threading;

    internal static class AsyncUtil {

        public static void WaitForAsyncResultCompletion(IAsyncResult asyncResult, HttpApplication app) {
            // based on HttpServerUtility.ExecuteInternal()

            if (!asyncResult.IsCompleted) {
                // suspend app lock while waiting, else might deadlock
                bool needToRelock = false;

                try {
                    // .NET 2.0+ will not allow a ThreadAbortException to be thrown while a
                    // thread is inside a finally block, so this pattern ensures that the
                    // value of 'needToRelock' is correct.
                    try { }
                    finally {
                        Monitor.Exit(app);
                        needToRelock = true;
                    }

                    WaitHandle waitHandle = asyncResult.AsyncWaitHandle;

                    if (waitHandle != null) {
                        waitHandle.WaitOne();
                    }
                    else {
                        while (!asyncResult.IsCompleted) {
                            Thread.Sleep(1);
                        }
                    }
                }
                finally {
                    if (needToRelock) {
                        Monitor.Enter(app);
                    }
                }
            }
        }

        public static AsyncCallback WrapCallbackForSynchronizedExecution(AsyncCallback callback, SynchronizationContext syncContext) {
            if (callback == null || syncContext == null) {
                return callback;
            }

            AsyncCallback newCallback = delegate(IAsyncResult asyncResult) {
                if (asyncResult.CompletedSynchronously) {
                    callback(asyncResult);
                }
                else {
                    // Only take the application lock if this request completed asynchronously,
                    // else we might end up in a deadlock situation.
                    syncContext.Sync(() => callback(asyncResult));
                }
            };

            return newCallback;
        }

    }
}
